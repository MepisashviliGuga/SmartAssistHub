## Pending Architectural Decisions

### Slug uniqueness
Slug.Generate() produces the base slug only.
Uniqueness enforcement is deferred to the Application layer
via ISlugUniquenessChecker — to be implemented when we
build the CreateTenant use case.

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