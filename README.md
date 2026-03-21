# SmartAssist Hub

A production-grade, multi-tenant AI assistant platform built with .NET 8 and Azure.
Companies onboard to the platform, upload their documents, and their users get an
intelligent AI assistant that answers questions based on those documents — privately,
securely, and at scale.

---

## Why this project exists

This is not a tutorial project. Every architectural decision here solves a real
production problem:

- **Multi-tenancy** with row-level isolation enforced at the query level — not by
  developer discipline but by the framework itself
- **RAG (Retrieval Augmented Generation)** with intelligent chunking strategy —
  semantic boundaries, not arbitrary character counts
- **Semantic caching** using vector similarity — "what is your refund policy?" and
  "how do I get a refund?" return the same cached answer
- **Cache stampede prevention** using probabilistic early expiry
- **The Outbox Pattern** for guaranteed message delivery across distributed services
- **Circuit breaker + exponential backoff with jitter** for resilience against
  Azure OpenAI downtime
- **Token-level streaming** with backpressure handling via SignalR

---

## Architecture

Clean Architecture with strict dependency rules enforced at the compiler level.
```
Domain          → no dependencies        (business entities)
Application     → Domain                 (use cases, interfaces)
Persistence     → Application + Domain   (EF Core, repositories)
Infrastructure  → Application + Domain   (Azure AI, Redis, Service Bus)
Api             → all                    (composition root, HTTP, SignalR)
```

---

## Tech stack

| Concern | Technology |
|---|---|
| Runtime | .NET 8, ASP.NET Core |
| AI | Azure OpenAI (GPT-4o), Semantic Kernel |
| Caching | Azure Cache for Redis |
| Database | Azure Cosmos DB + Azure SQL |
| Messaging | Azure Service Bus |
| Auth | Azure AD B2C — OAuth2 / OIDC |
| Gateway | Azure API Management |
| Hosting | Azure Container Apps |
| Observability | Azure Application Insights |
| CI/CD | GitHub Actions + Azure Container Registry |

---

## Project status

| Phase | Status |
|---|---|
| Solution structure + Domain entities | In progress |
| Application layer + use cases | Pending |
| Persistence layer + repositories | Pending |
| Infrastructure — AI + RAG pipeline | Pending |
| Infrastructure — Caching + resilience | Pending |
| Infrastructure — Messaging + Outbox | Pending |
| API layer + SignalR streaming | Pending |
| Authentication + multi-tenancy | Pending |
| Azure deployment + CI/CD | Pending |
| Frontend | Pending |

---

## Running locally

> Prerequisites: .NET 8, Docker Desktop, Visual Studio 2022
```bash
# Start local dependencies (Redis, SQL Server)
docker-compose up -d

# Run the API
cd src/Presentation/SmartAssistHub.Api
dotnet run
```

---

## Key concepts explained

Each of the hard architectural decisions in this project has a dedicated explanation
in the `/docs` folder. If you're reviewing this project and want to understand why
a specific decision was made, start there.
```

---

## The /docs folder

Notice the README mentions a `/docs` folder. Create it now — right-click solution → **Add** → **New Solution Folder** → name it `docs`. We'll add a markdown file to it for each major concept as we implement it. By the end of the project this folder becomes a technical journal that proves you understand every decision.

---

## Commit and push

Go to **Git** → **Commit or Stash**. Write this commit message:
```
chore: add README and docs folder structure
