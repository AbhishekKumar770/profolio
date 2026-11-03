# Profolio — Product Roadmap

## Product direction

Profolio starts as a focused dynamic portfolio builder for software developers. The goal is to ship a complete, polished product that can be demonstrated in a real portfolio and discussed in technical interviews.

**MVP audience:** software developers only (for example .NET, Java, Python, frontend and full-stack developers).

**Public URL:** `profolio.com/{username}`.

**Templates:** two responsive templates using the same portfolio content model.

## MVP scope

### 1. Account and ownership

- Register, login and logout.
- One account owns one tenant.
- One developer profile per tenant.
- Secure tenant resolution and authorization.

### 2. Developer profile

- Name, headline, biography and location.
- Profile image.
- Social links such as GitHub and LinkedIn.
- Resume link or download if included in the implementation schedule.

### 3. Portfolio content

- Technical skills.
- Work experience.
- Education.
- Projects with title, description, technologies, GitHub URL and live URL.
- Add, edit, reorder and remove supported content.

### 4. Website builder

- Enable/disable supported sections.
- Reorder sections.
- Choose between two responsive templates.
- Basic theme customization within supported design options.
- Preview the draft before publishing.

### 5. Publishing

- Save edits as draft.
- Preview draft for the owner.
- Publish an immutable versioned snapshot.
- Public visitors see published content only.
- Public route resolves by normalized username.

## MVP acceptance criteria

The MVP is ready for a first release when:

1. A developer can register and sign in.
2. The account can create and manage one developer portfolio.
3. Profile, skills, experience, education and projects can be managed through the dashboard.
4. The developer can choose either of the two templates.
5. The public portfolio renders dynamically at the username URL.
6. Draft changes are not visible publicly until published.
7. Publishing creates a new immutable snapshot.
8. A user cannot read or modify another tenant's private dashboard data.
9. Core flows have automated tests.
10. The application can be run and deployed using documented instructions.

## Suggested delivery plan

This is a target sequence, not a guaranteed deadline.

| Milestone | Work |
|---|---|
| M0 — Repository foundation | Git workflow, README, architecture and product docs |
| M1 — Backend foundation | Solution structure, Identity, persistence and tenant context |
| M2 — Profile APIs | Developer profile and social links |
| M3 — Portfolio APIs | Skills, experience, education and projects |
| M4 — Dashboard | Next.js authentication and content management |
| M5 — Public portfolio | Username routing and two templates |
| M6 — Publishing | Draft, preview, immutable snapshots |
| M7 — Release hardening | Tenant-isolation tests, validation, Docker and deployment |

A focused 8-week plan may be possible with consistent daily effort, but quality, testing and deployment take priority over an arbitrary date.

## Deferred — not part of MVP

Keep these as future possibilities only:

- Other professions and profession-specific onboarding
- Subscription tiers, billing and payments
- Advanced analytics
- Custom domains
- AI content generation
- Professional discovery/search
- Networking and connections
- Hiring and job applications
- Appointment booking
- Team memberships and collaboration
- Multilingual portfolio content
- Advanced platform administration

Do not create database tables or infrastructure for deferred features before an approved use case requires them.

## Current implementation status

- [x] Public GitHub repository created
- [x] Local repository cloned and Git remote configured
- [ ] Repository documentation committed and pushed
- [ ] Backend solution created
- [ ] Frontend application created
- [ ] Database migrations created
- [ ] MVP features implemented
- [ ] Automated tests added
- [ ] Deployment completed

Update this checklist as work is actually completed. Never describe planned functionality as implemented in a resume or project demo.
