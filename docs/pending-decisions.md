## Pending Architectural Decisions

### Slug uniqueness
Slug.Generate() produces the base slug only.
Uniqueness enforcement is deferred to the Application layer
via ISlugUniquenessChecker — to be implemented when we
build the CreateTenant use case.

### Document and User collections on Tenant
Currently commented out pending User and Document entity creation.
Will be uncommented as each entity is added.