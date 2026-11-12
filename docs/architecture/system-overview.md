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

ASP.NET Core 10 Web API in a Modular Monolith, organized with Clean Architecture boundaries:

- Domain: entities, invariants and domain rules (zero external dependencies)
- Application: use cases, validation and abstractions (`IApplicationDbContext`, `ITenantContext`)
- Infrastructure: EF Core 10 (Npgsql), Identity, PostgreSQL schemas and external adapters
- API: HTTP endpoints, authentication, middleware and composition

Use pragmatic CQRS: separate read and write use cases where it improves clarity, without introducing unnecessary messaging, generic repositories, or distributed infrastructure.

### Database

One shared PostgreSQL database with logical schemas:

- `identity`
- `tenancy`
- `profile`
- `portfolio`
- `website`
- `media`
- `admin`

Use GUID identifiers (`uuid`) for business entities and ASP.NET Core Identity string IDs for users. Tenant-owned records carry `TenantId`. JSON snapshots and flexible configurations use native PostgreSQL `jsonb`. Hosting target is Neon / Render, designed to remain fully portable to any Docker-compatible VPS.

### Tenant isolation

Initial model: one account owns one tenant and one developer profile.

Tenant resolution is performed strictly on the server: by backend middleware resolving the authenticated user for dashboard requests, and from the normalized username for public requests. The client must never supply or be trusted with a tenant ID.

Defense in depth:

1. Server-derived tenant context (resolve and validate tenant from authenticated user identity).
2. Fail-closed EF Core global query filters applied to all tenant-owned entities.
3. Enforce authorization and resource ownership in application use cases on both reads and writes.
4. Validate tenant-aware writes and relationships.
5. Integration tests explicitly proving tenant isolation and preventing cross-tenant leakage.

Global query filters are a safeguard, not the sole security boundary. Administrative bypasses must be explicit, rare and tightly controlled.

## Core request flows

### Dashboard write

Browser -> authenticated API -> tenant context -> authorization -> application use case -> EF Core (Npgsql) -> PostgreSQL.

### Public portfolio read

Username route -> resolve active tenant -> load current published snapshot (`jsonb`) -> render selected template. Do not query draft records for anonymous public requests.

### Publish

Validate draft and referenced media/template -> create immutable versioned snapshot (`jsonb`) -> update published pointer in one database transaction. If publication fails, the previous published version remains active.

## Content and presentation

Keep portfolio content separate from its presentation:

- Profile, projects, skills, experience and education are structured relational data.
- Section ordering, visibility and template-specific settings are configuration.
- `jsonb` is reserved for validated, versioned flexible configuration and published snapshots—not a replacement for core relational entities.
- Two templates consume the same normalized portfolio model.

## Media

Store file bytes in object storage when media support is implemented. PostgreSQL stores metadata and ownership. Validate tenant ownership before associating media with profile images or project images.

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
