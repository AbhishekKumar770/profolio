# Profolio — System Overview

## Status

Architecture baseline for the developer-only MVP. This document describes intended design; it is not evidence that these components have been implemented.

## Product boundary

The first release serves software developers only. A signed-in account manages one developer profile and one public portfolio. The public route is `/{username}` (for example, `profolio.com/abhishek`).

Two responsive templates are included in MVP. Users manage content and choose a template; they do not build arbitrary page layouts.

## Components

### Frontend

Next.js + TypeScript provides:

- Authenticated dashboard and profile editor
- Project, skill, experience and education management
- Draft preview
- Public username route
- Two portfolio templates

Public pages must render only published data. Dashboard preview is an authenticated path and may render draft data.

### Backend

ASP.NET Core Web API in a Modular Monolith, organized with Clean Architecture boundaries:

- Domain: entities, invariants and domain rules
- Application: use cases, validation and abstractions
- Infrastructure: EF Core, Identity, storage and external adapters
- API: HTTP endpoints, authentication, middleware and composition

Use pragmatic CQRS: separate read and write use cases where it improves clarity, without introducing unnecessary messaging or distributed infrastructure.

### Database

One shared SQL Server database with logical schemas:

- `Identity`
- `Tenancy`
- `Profile`
- `Portfolio`
- `Website`
- `Media`
- `Admin`

Use GUID identifiers for business entities and ASP.NET Core Identity string IDs for users. Tenant-owned records carry `TenantId`.

### Tenant isolation

Initial model: one account owns one tenant and one developer profile.

Tenant resolution is performed by backend middleware from the authenticated account for dashboard requests, and from the normalized username for public requests. The client must never choose an arbitrary tenant ID.

Defense in depth:

1. Resolve and validate tenant context.
2. Enforce authorization and resource ownership in application use cases.
3. Apply EF Core global query filters to tenant-owned entities.
4. Validate tenant-aware writes and relationships.
5. Add tenant-isolation integration tests.

Global filters are not the sole security boundary. Administrative bypasses must be explicit and tightly controlled.

## Core request flows

### Dashboard write

Browser -> authenticated API -> tenant context -> authorization -> application use case -> EF Core -> SQL Server.

### Public portfolio read

Username route -> resolve active tenant -> load current published snapshot -> render selected template. Do not query draft records for anonymous public requests.

### Publish

Validate draft and referenced media/template -> create immutable versioned snapshot -> update published pointer in one database transaction. If publication fails, the previous published version remains active.

## Content and presentation

Keep portfolio content separate from its presentation:

- Profile, projects, skills, experience and education are structured relational data.
- Section ordering, visibility and template-specific settings are configuration.
- JSON is reserved for validated, versioned flexible configuration—not a replacement for core relational entities.
- Two templates consume the same normalized portfolio model.

## Media

Store file bytes in object storage when media support is implemented. SQL Server stores metadata and ownership. Validate tenant ownership before associating media with profile images or project images.

## Quality attributes

- Tenant isolation and authorization
- Accessible, responsive public pages
- Server-side validation
- Optimistic concurrency for editable records where appropriate
- Structured logging without secrets or unnecessary personal data
- Unit, integration and end-to-end tests
- Secure secret management and reviewed database migrations

## Deferred architecture

Do not build microservices, Redis, Kafka, billing, analytics, custom domains, AI, networking or hiring for the MVP. Revisit only when product needs justify the complexity.
