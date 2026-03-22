## Pending Architectural Decisions

### Slug uniqueness
Slug.Generate() produces the base slug only.
Uniqueness enforcement is deferred to the Application layer
via ISlugUniquenessChecker — to be implemented when we
build the CreateTenant use case.

### Document and User collections on Tenant
Currently commented out pending User and Document entity creation.
Will be uncommented as each entity is added.

### Token usage coordination between Conversation and Tenant

When a message is completed, token usage must be recorded on BOTH:
- Conversation.TotalTokensUsed (already handled in domain)
- Tenant.RecordTokenUsage() (must be called from Application layer)

This coordination belongs in the SendMessage use case in the
Application layer — not in the Domain entities themselves.
Both saves must happen in the same transaction to prevent
inconsistent token counts.

### IsEmailVerified — source of truth

Azure AD B2C is the source of truth for email verification.
User.IsEmailVerified is a cached business flag only.
On every authenticated request the API layer syncs this value
from the email_verified JWT claim if out of sync.
Never trust the DB value over the JWT claim.