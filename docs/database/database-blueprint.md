# Profolio — PostgreSQL Database Blueprint

## Status

This is the planned MVP database design targeting PostgreSQL (hosted on Neon or Render, portable to any Docker-compatible VPS). No database schema or migrations have been implemented yet. Keep this blueprint synchronized with reviewed EF Core configurations and migrations as implementation progresses.

## Scope decisions

- Audience: software developers only.
- One account -> one tenant -> one developer profile.
- One shared PostgreSQL database with multiple logical schemas.
- GUID (`uuid`) primary keys for business entities; ASP.NET Core Identity user IDs remain strings.
- Tenant-owned records include `TenantId`.
- Public portfolio route: `/{username}`.
- Project slugs are unique per tenant.
- Two templates share one content model.
- Standard content is relational; flexible settings and published snapshots use validated, versioned `jsonb`.
- Publishing creates immutable snapshots stored as `jsonb`.

## Logical schemas

| Schema | MVP responsibility |
|---|---|
| `identity` | ASP.NET Core Identity tables |
| `tenancy` | Tenants and username/slug resolution |
| `profile` | Developer profile and social links |
| `portfolio` | Sections, projects, technologies, skills, experience, education |
| `website` | Two templates, website settings and publishing snapshots |
| `media` | Media metadata and content relationships |
| `admin` | Minimal audit records where needed |

Billing, analytics, discovery, networking and hiring schemas are deferred.

## Proposed MVP tables

### Identity

Use ASP.NET Core Identity for users, password hashing, roles and external login support if Google login is included. Do not create a custom password table.

### Tenancy

- `Tenants`: Id (`uuid`), OwnerUserId (`text`), DisplayName (`varchar(100)`), Status (`varchar(50)`), CreatedAtUtc (`timestamptz`), UpdatedAtUtc (`timestamptz`).
- `TenantUsernames`: Id (`uuid`), TenantId (`uuid`), normalized Username (`varchar(50)`), IsPrimary (`boolean`), CreatedAtUtc (`timestamptz`).

Enforce one tenant per owner for MVP and a globally unique normalized username. Reserve system routes such as `api`, `login`, `register`, `admin`, `settings` and `support`. Validate username syntax and reserved names in application logic, with database constraints/indexes as defense in depth.

### Profile

- `DeveloperProfiles`: one row per TenantId; FullName (`varchar(100)`), Headline (`varchar(200)`), Bio (`text`), Location (`varchar(100)`), profile media reference (`uuid`).
- `SocialLinks`: TenantId (`uuid`), platform (`varchar(50)`), label (`varchar(100)`), URL (`text`), sort order (`integer`).

The MVP profile is developer-specific. Do not add profession taxonomy until multiple professions become a real requirement.

### Portfolio

- `SectionCatalog`: system-supported section definitions such as About, Skills, Experience, Projects, Education and Contact.
- `TenantSections`: sections enabled by a tenant, title, sort order, visibility and validated settings.
- `Projects`: TenantId (`uuid`), title (`varchar(150)`), slug (`varchar(150)`), descriptions (`text`), repository/demo URLs (`text`), featured flag (`boolean`), sort order (`integer`), soft-delete metadata.
- `ProjectTechnologies`: tenant-aware project-to-technology relationship.
- `Technologies` or `Skills`: developer skill names (`varchar(100)`) and optional proficiency/sort order.
- `Experiences`: company (`varchar(150)`), role (`varchar(150)`), dates, description (`text`) and sort order (`integer`).
- `Educations`: institution (`varchar(150)`), qualification (`varchar(150)`), dates, description (`text`).

Add certifications or testimonials only if MVP user stories require them; they are not launch blockers.

### Website

- `Templates`: exactly two active MVP templates, represented by stable template keys.
- `TemplateVersions`: immutable template/config version metadata if template evolution requires it.
- `TenantWebsiteSettings`: selected template, theme tokens and website-level configuration (`jsonb`).
- `PortfolioVersions`: TenantId (`uuid`), version number (`integer`), snapshot JSON (`jsonb`), snapshot schema version (`integer`), publication metadata (`timestamptz`).
- `PublishingStates`: one row per tenant, pointing to the currently published version.

The snapshot contains resolved public content, section order/visibility, selected template and version, theme and SEO metadata. It must not contain private fields or draft-only data.

### Media

- `MediaFiles`: TenantId (`uuid`), storage key (`text`), file name (`varchar(255)`), content type (`varchar(100)`), size (`bigint`), dimensions and upload timestamp (`timestamptz`).
- `MediaUsages`: explicit media-to-content relationship, or typed junction tables for important relationships.

Store bytes in object storage, not PostgreSQL. Validate tenant ownership and allowed file types/sizes on upload and association.

### Admin

Add a minimal audit log only for security-sensitive or important business actions. Avoid building a full platform administration subsystem for MVP.

## Relationship and isolation rules

- Every tenant-owned entity has `TenantId`.
- Use composite alternate keys such as `(TenantId, Id)` on important parents and composite foreign keys from child records where practical.
- Use unique `(TenantId, Slug)` for active project slugs.
- Use a unique index on normalized username.
- Enforce one profile and one publishing-state row per tenant.
- Never accept TenantId as an authorization claim merely because the browser supplied it.
- Validate ownership for media and every parent-child relationship.

## Publishing consistency

1. Load draft content for the authenticated tenant.
2. Validate required fields, visibility, media ownership and template availability.
3. Build a versioned public snapshot.
4. Insert the immutable snapshot and update the published-version pointer in one database transaction.
5. Keep the previous publication active if validation or persistence fails.
6. Rollback by publishing a new version derived from a prior snapshot; never mutate historical snapshots.

## PostgreSQL conventions

- Business IDs: `uuid` (represented as `Guid` in .NET); Identity user IDs: `text` / `varchar(450)`.
- UTC timestamps: `timestamptz` (`timestamp with time zone`).
- Concurrency: Optimistic concurrency using PostgreSQL-supported mechanisms (e.g. evaluating `xmin` system column via Npgsql or an explicit concurrency token).
- JSON: Native `jsonb` with application-level schema validation.
- Use bounded `varchar(n)` for structured strings; `text` for unrestricted text/descriptions.
- Index tenant-scoped query patterns; avoid indexing every column.
- Soft-delete user content only where restoration/retention is useful.

## Deferred database work

No subscription/payment, analytics event warehouse, custom domain, professional discovery, connection, job application, appointment or AI tables in MVP. Add them only with a separately approved feature scope.
