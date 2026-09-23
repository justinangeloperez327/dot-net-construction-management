# Authentication

## Approach

Authentication uses ASP.NET Core Identity with secure cookie authentication.

This is a server-side Blazor application, so browser authentication uses the normal ASP.NET Core cookie model rather than storing bearer tokens in JavaScript.

## Boundaries

Identity persistence is an Infrastructure concern:

```text
Web
  ├── Login/logout presentation
  └── Current-user adapter
          ↓
Application
  └── ICurrentUser

Infrastructure
  ├── ApplicationUser
  ├── ASP.NET Core Identity
  └── Identity EF Core stores
          ↓
ApplicationDbContext
```

Domain does not depend on ASP.NET Core Identity.

## Current scope

Group 3 includes:

- cookie authentication
- Identity persistence
- sign-in
- sign-out
- lockout after repeated failed attempts
- current-user abstraction
- protected Blazor routes
- access-denied page

Self-registration is intentionally not enabled. User provisioning, role assignment, and permission management belong to Group 4.

## Cookie policy

The application cookie is:

- HTTP-only
- SameSite=Lax
- sliding
- eight-hour lifetime

Production must use HTTPS.

## Password policy

Initial policy:

- minimum 10 characters
- uppercase required
- lowercase required
- digit required
- non-alphanumeric character required

These rules can be changed when the organization's actual password policy is confirmed.

## Migrations

Authentication introduces the first persisted application schema. The Identity migration is stored under:

```text
src/Infrastructure/Persistence/Migrations
```

Production migrations remain an explicit deployment step and are not automatically executed during application startup.
