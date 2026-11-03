# Profolio — SQL Server Database Blueprint

## Status

This is the planned MVP database design. No database schema or migrations have been implemented yet. Keep this blueprint synchronized with reviewed EF Core configurations and migrations as implementation progresses.

## Scope decisions

- Audience: software developers only.
- One account -> one tenant -> one developer profile.
- One shared SQL Server database with multiple logical schemas.
- GUID primary keys for business entities; ASP.NET Core Identity user IDs remain strings.
- Tenant-owned records include `TenantId`.
- Public portfolio route: `/{username}`.
- Project slugs are unique per tenant.
- Two templates share one content model.
- Standard content is relational; flexible settings use validated, versioned JSON.
- Publishing creates immutable snapshots.

## Logical schemas

| Schema | MVP responsibility |
|---|---|
| `Identity` | ASP.NET Core Identity tables |
| `Tenancy` | Tenants and username/slug resolution |
| `Profile` | Developer profile and social links |
| `Portfolio` | Sections, projects, technologies, skills, experience, education |
| `Website` | Two templates, website settings and publishing snapshots |
| `Media` | Media metadata and content relationships |
| `Admin` | Minimal audit records where needed |

Billing, analytics, discovery, networking and hiring schemas are deferred.

## Proposed MVP tables

### Identity

Use ASP.NET Core Identity for users, password hashing, roles and external login support if Google login is included. Do not create a custom password table.

### Tenancy

- `Tenants`: Id, OwnerUserId, DisplayName, Status, CreatedAtUtc, UpdatedAtUtc, RowVersion.
- `TenantUsernames` (or `TenantDomains` if domain support is intentionally modeled): Id, TenantId, normalized Username, IsPrimary, CreatedAtUtc.

Enforce one tenant per owner for MVP and a globally unique normalized username. Reserve system routes such as `api`, `login`, `register`, `admin`, `settings` and `support`. Validate username syntax and reserved names in application logic, with database constraints/indexes as defense in depth.

### Profile

- `ProfessionalProfiles`: one row per TenantId; full name, headline, biography, location, profile media reference.
- `SocialLinks`: TenantId, platform, label, URL, sort order.

The MVP profile is developer-specific. Do not add profession taxonomy until multiple professions become a real requirement.

### Portfolio

- `SectionCatalog`: system-supported section definitions such as About, Skills, Experience, Projects, Education and Contact.
- `TenantSections`: sections enabled by a tenant, title, sort order, visibility and validated settings.
- `Projects`: TenantId, title, slug, descriptions, repository/demo URLs, featured flag, sort order, soft-delete metadata.
- `ProjectTechnologies`: tenant-aware project-to-technology relationship.
- `Technologies` or `Skills`: developer skill names and optional proficiency/sort order.
- `Experiences`: company, role, dates, description and sort order.
- `Educations`: institution, qualification, dates and description.

Add certifications or testimonials only if MVP user stories require them; they are not launch blockers.

### Website

- `Templates`: exactly two active MVP templates, represented by stable template keys.
- `TemplateVersions`: immutable template/config version metadata if template evolution requires it.
- `TenantWebsiteSettings`: selected template, theme tokens and website-level configuration.
- `PortfolioVersions`: TenantId, version number, snapshot JSON, snapshot schema version, publication metadata.
- `PublishingStates`: one row per tenant, pointing to the currently published version.

The snapshot contains resolved public content, section order/visibility, selected template and version, theme and SEO metadata. It must not contain private fields or draft-only data.

### Media

- `MediaFiles`: TenantId, storage key, file name, content type, size, dimensions and upload timestamp.
- `MediaUsages`: explicit media-to-content relationship, or typed junction tables for important relationships.

Store bytes in object storage, not SQL Server. Validate tenant ownership and allowed file types/sizes on upload and association.

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
4. Insert the immutable snapshot and update the published-version pointer in one transaction.
5. Keep the previous publication active if validation or persistence fails.
6. Rollback by publishing a new version derived from a prior snapshot; never mutate historical snapshots.

## SQL conventions

- Business IDs: `UNIQUEIDENTIFIER`; Identity user IDs: `NVARCHAR(450)` unless configured otherwise.
- UTC timestamps: `DATETIME2(3)`.
- Concurrency: `ROWVERSION` where concurrent editing matters.
- JSON: `NVARCHAR(MAX)` with `ISJSON` checks plus application-level schema validation.
- Use bounded `NVARCHAR(n)` for ordinary strings.
- Index tenant-scoped query patterns; avoid indexing every column.
- Prefer sequential GUID generation consistently where appropriate.
- Soft-delete user content only where restoration/retention is useful.

## Deferred database work

No subscription/payment, analytics event warehouse, custom domain, professional discovery, connection, job application, appointment or AI tables in MVP. Add them only with a separately approved feature scope.
