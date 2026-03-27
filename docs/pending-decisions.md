## Pending Architectural Decisions

### Slug uniqueness
~~Deferred to Application layer via ISlugUniquenessChecker~~
RESOLVED: SlugExistsAsync and GenerateUniqueSlugAsync added
directly to ITenantRepository. Implemented in Persistence layer.

### Token usage coordination between Conversation and Tenant
When a message is completed, token usage must be recorded on BOTH:
- Conversation.TotalTokensUsed (handled synchronously in domain)
- Tenant.RecordTokenUsage() (handled asynchronously via Outbox)

These intentionally do NOT share a transaction — they are separate
aggregates. Eventual consistency is accepted for token tracking.

RESOLVED: MessageCompletedEvent carries TokensUsed.
Handler in Application layer loads tenant and calls
tenant.RecordTokenUsage(event.TokensUsed).

### IsEmailVerified — source of truth
Azure AD B2C is the source of truth for email verification.
User.IsEmailVerified is a cached business flag only.
On every authenticated request the API layer syncs this value
from the email_verified JWT claim if out of sync.
Never trust the DB value over the JWT claim.

### Token usage eventual consistency
Conversation.TotalTokensUsed and Tenant.TokensUsedThisMonth
are updated in separate transactions intentionally.
There is a brief eventual consistency window of approximately
10-30 seconds where the tenant budget counter may be slightly
behind reality. This is an accepted tradeoff.
For hard budget enforcement, add a small buffer to the limit
or implement a pre-check against Conversation.TotalTokensUsed
at the API layer.

### User registration consistency — Azure AD B2C + Domain DB

Source of truth: Azure AD B2C owns identity.
Our DB owns business data (role, tenant, preferences).

Consistency model: Eventual consistency — accepted tradeoff.

Flow:
    User registers on Azure AD B2C
    → Azure API Connector calls our pre-registration webhook
        → Our API validates against domain rules
        → If invalid: return error → Azure rejects registration
        → If valid: return success → Azure creates auth account
    → Azure calls post-registration webhook
    → API sends RegisterUserCommand
    → Handler creates User domain entity idempotently

Recovery strategies:
    1. Idempotent handler — GetByExternalIdAsync check first
       if user already exists in DB, return existing silently
    2. Webhook retry — Azure AD B2C retries failed webhook calls
    3. Login-time sync — on every login, if user missing from DB,
       create on the fly from JWT claims (email, name, tenantId)

RegisterUserCommand contains no Azure AD B2C calls.
Application layer has zero knowledge of Azure AD B2C.

### User identity — ExternalId from Azure AD B2C

User.ExternalId stores the Azure AD B2C ObjectId (sub claim).
This is the primary identity key for idempotency checks.
Email is a property, not the identity key.
ExternalId never changes even if email changes.

IUserRepository.GetByExternalIdAsync is the idempotency check
in RegisterUserCommandHandler.

Email uniqueness is still enforced per tenant — within one tenant
two users cannot share the same email. This is a tenant-scoped
constraint, not a global one.

### Validation synchronization across Azure AD B2C and domain

Problem: Azure AD B2C and our domain have different validation rules.
A user could be created in Azure but rejected by our system.

Solution: Azure AD B2C API Connector (pre-registration hook).
Our API validates against domain rules BEFORE Azure creates the account.
If our API rejects → Azure shows error → no account created anywhere.

Layers:
    1. Azure portal — basic email/password rules
    2. Pre-registration API Connector — our business rules (main gate)
    3. Domain entities — same rules enforced again (defense in depth)
    4. Database constraints — race condition protection

Implementation: Pre-registration endpoint built during Phase 5 (API layer).

### ForbiddenException vs UnauthorizedException

UnauthorizedException → HTTP 401 → user is not authenticated
ForbiddenException → HTTP 403 → user is authenticated but not allowed

Use ForbiddenException when:
    - Tenant is inactive
    - User does not have required role
    - User tries to access another tenant's data

Use UnauthorizedException when:
    - No JWT token present
    - JWT token is invalid or expired

### SendMessage — token recording strategy

Token usage is recorded via IncrementTokenUsageAsync — an atomic
SQL UPDATE that runs after the conversation is saved.

This replaces the original tenant.RecordTokenUsage() + UpdateAsync
approach which had a race condition under concurrent load.

If IncrementTokenUsageAsync fails, MessageCompletedEvent will
eventually trigger token recording via the event handler as fallback.

MessageCompletedEvent still fires for analytics and other side effects.

### SendMessage — token counting optimization

Currently calls CountTokensAsync after streaming completes.
Azure OpenAI streaming response includes usage metadata
with exact token count at the end of the stream.

Fix in Infrastructure layer: IAiService.StreamCompletionAsync
should return both tokens and usage count together.
Consider returning a StreamingResult record instead of
IAsyncEnumerable<string> directly.

### Document upload — synchronous vs asynchronous blob upload

Considered: async blob upload via Worker after DB save
    + cleaner consistency model
    - file stream unavailable after HTTP request completes
    - poor UX (user sees pending instead of uploaded)

Decision: synchronous blob upload in handler
    File upload happens during HTTP request
    Compensating transaction handles DB failure
    If compensating delete also fails — blob is logged as orphaned
    Nightly cleanup handles orphaned blobs
    Revisit if file size limits increase beyond 100MB
    or if upload latency becomes a problem

### TenantPlanUpdatedEvent — no handler yet

TenantPlanUpdatedEvent is raised when a tenant changes plan.
Currently no handler exists — token limit is updated
synchronously inside UpdatePlan() domain method.

Future handlers needed:
    - Billing system notification
    - Owner confirmation email

Handler implementation deferred to Infrastructure layer
when billing integration is added.

### Concurrency control strategy

Tenant entity has RowVersion for optimistic concurrency on
general field updates.

Token usage increments use atomic SQL UPDATE rather than
read-modify-write to prevent lost updates under concurrent load:
    UPDATE Tenants SET TokensUsedThisMonth += @tokens WHERE Id = @id

This is more performant than pessimistic locking and more
correct than optimistic concurrency for high-frequency counters.

IncrementTokenUsageAsync added to ITenantRepository.
SendMessageCommandHandler updated to use atomic increment
instead of tenant.RecordTokenUsage() + UpdateAsync.

### Vector embedding storage strategy

DocumentChunk.Embedding (float[]) must NEVER be loaded as part
of standard Document aggregate queries. Loading 1000 chunks with
1536 floats each allocates ~6MB on the Large Object Heap causing
GC pauses.

Strategy:
    - Embeddings stored in Azure AI Search (primary vector store)
    - EF Core configuration ignores the Embedding property via Ignore()
    - DocumentChunk.Embedding used only during processing pipeline
    - Never included in standard repository queries
    - GetEmbeddingSnapshot() used by Infrastructure when sending to Azure AI Search

### Pagination strategy — offset vs cursor

Standard queries (Tenants, Users, Documents):
    Offset pagination is acceptable — these lists grow slowly

Messages within a conversation:
    Cursor-based pagination required — conversations can have
    thousands of messages. Use CreatedAt as cursor:
    WHERE CreatedAt < @cursor ORDER BY CreatedAt DESC
    Built in Persistence layer when implementing conversation queries.

### DbUpdateConcurrencyException retry strategy

When RowVersion conflict is detected EF Core throws
DbUpdateConcurrencyException.

Strategy: ConcurrencyRetryBehavior in MediatR pipeline.
    - Catches DbUpdateConcurrencyException
    - Retries up to 3 times using Polly
    - Logs each retry attempt
    - Returns HTTP 409 after 3 failed attempts

Implementation deferred to Phase 5 (API layer).