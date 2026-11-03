# Profolio

**Dynamic Developer Portfolio Platform**

Profolio is a portfolio builder focused initially on software developers. It lets a developer manage professional information in a dashboard and publish a responsive portfolio at a public username URL.

> **Project status:** Planning and repository foundation. Application features are not implemented yet.

## MVP goal

Build one complete, polished portfolio experience for software developers before expanding into a broader professional platform.

### MVP features

- Account registration, login and logout
- One developer profile per account
- Profile details: name, headline, biography, location and profile image
- Technical skills
- Work experience and education
- Projects with descriptions, technologies, GitHub and live-demo links
- Two responsive portfolio templates
- Username-based public URL: `profolio.com/{username}`
- Dashboard editing, preview and explicit publishing
- Public visitors see published content only

### Explicitly out of scope for MVP

Multi-profession support, subscription plans and payments, analytics, custom domains, AI features, networking, hiring, appointments, team memberships and multilingual support. These remain future roadmap possibilities, not MVP commitments.

## Planned technology stack

| Area | Technology |
|---|---|
| Frontend | Next.js + TypeScript |
| Backend | ASP.NET Core Web API |
| Architecture | Modular Monolith + Clean Architecture |
| Database | Microsoft SQL Server |
| Data access | Entity Framework Core |
| API style | REST + OpenAPI |
| Authentication | ASP.NET Core Identity |
| UI data | TanStack Query where appropriate |
| Deployment | Docker; CI/CD planned |

Versions and hosting providers will be selected and documented when implementation begins.

## High-level architecture

```text
Developer
   |
   v
Next.js Dashboard -----> ASP.NET Core Web API
                              |
                              v
                         Application
                              |
                              v
                         SQL Server
                              |
                       Draft portfolio data
                              |
                       Publish operation
                              |
                       Versioned snapshot
                              |
                              v
Next.js public page <--- Published portfolio
profolio.com/{username}
```

The initial product uses one shared database. Each account owns one tenant and one developer portfolio. Tenant-owned records are scoped by `TenantId`; tenant resolution, authorization and data-access filters work together to prevent cross-tenant access.

## Repository layout

```text
docs/
  architecture/
  database/
  product/
src/
  backend/
  frontend/
tests/
```

Detailed decisions are maintained in:

- [System overview](docs/architecture/system-overview.md)
- [Database blueprint](docs/database/database-blueprint.md)
- [Product roadmap](docs/product/product-roadmap.md)

## Local development prerequisites

Install these when the corresponding implementation phase starts:

- Git
- .NET SDK (version to be pinned when backend setup begins)
- Node.js LTS and npm
- SQL Server (local instance or container)
- Docker Desktop (for container workflows)
- A code editor such as Visual Studio Code or Visual Studio

No application runtime is available yet because the MVP implementation has not started.

## Engineering principles

- Keep the MVP small and finish vertical slices end to end.
- Keep domain and application logic independent of infrastructure.
- Never trust a client-supplied `TenantId`.
- Store secrets in local environment configuration or a secret manager; never commit them.
- Validate tenant ownership on reads, writes and media associations.
- Keep published snapshots immutable.
- Add unit and integration tests, especially tenant-isolation tests.
- Document implemented behavior separately from planned work.

## Git workflow

- `main`: stable, reviewable work.
- `feature/<short-description>`: feature development.
- `fix/<short-description>`: bug fixes.
- Use focused commits, for example:
  - `chore: initialize repository structure`
  - `feat: add developer profile management`
  - `test: cover tenant isolation`
  - `docs: update database blueprint`

Use pull requests for meaningful changes and keep the README status accurate.

## Roadmap

1. Repository and architecture foundation
2. Backend foundation, Identity and tenant isolation
3. Developer profile and portfolio content APIs
4. Next.js dashboard and portfolio editor
5. Two public templates and username routing
6. Draft, preview and publishing snapshots
7. Testing, security hardening and deployment

See [Product roadmap](docs/product/product-roadmap.md) for scope and acceptance criteria.

## License

No open-source license has been selected yet. Until a license is added, the repository being public does not automatically grant permission to reuse its code.
