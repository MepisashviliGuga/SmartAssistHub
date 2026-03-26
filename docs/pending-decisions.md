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

### SendMessage — synchronous token recording exception

In SendMessage handler, tenant.RecordTokenUsage() is called
synchronously in the same transaction as the conversation save.

This is an intentional exception to the eventual consistency
rule for token tracking. The budget check happens at request
start — if we defer token recording to the Outbox, concurrent
requests could all pass the budget check before any of them
records usage, allowing budget overruns.

MessageCompletedEvent still fires via domain events for
analytics and other side effects. Only the budget-critical
RecordTokenUsage is handled synchronously.

### Document upload — synchronous vs asynchronous blob upload

Considered: async blob upload via Worker after DB save
    + cleaner consistency model
    - file stream unavailable after HTTP request completes
    - poor UX (user sees pending instead of uploaded)

Decision: synchronous blob upload in handler
    File upload happens during HTTP request
    Compensating transaction handles DB failure
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