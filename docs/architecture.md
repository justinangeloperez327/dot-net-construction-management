# Architecture

## Clean Architecture dependency rule

The application is divided into four layers.

```text
Domain
  ^
  |
Application
  ^
  |
Infrastructure      Web
```

The diagram represents dependency direction, not runtime call direction.

### Domain

Contains enterprise and construction-domain rules. It must not reference Application, Infrastructure, ASP.NET Core, Blazor, Entity Framework Core, SQL Server, or any delivery mechanism.

### Application

Contains use cases and interfaces required by those use cases. It may reference Domain only.

### Infrastructure

Contains implementations for persistence, identity, file storage, email, and external systems. It may reference Application and Domain.

### Web

Contains the ASP.NET Core host and Blazor UI. It is the composition root and may reference Application and Infrastructure.

## Development approach

Business capabilities are implemented as vertical slices across the layers. We do not complete one entire architectural layer before moving to the next.

For example, a future Create Project slice will include:

```text
Domain model/rules
        ↓
Application use case
        ↓
Infrastructure persistence
        ↓
Blazor page
        ↓
Tests
```

## Rules

1. Business rules do not belong in Blazor components.
2. Domain does not reference infrastructure packages.
3. Application does not reference Infrastructure or Web.
4. Infrastructure implements abstractions owned by Application when an abstraction is justified.
5. Do not add a generic repository, Unit of Work wrapper, mediator, event bus, or mapping library by default.
6. Add abstractions only when they protect a real boundary or remove meaningful duplication.
7. Build deployable vertical slices rather than large unfinished horizontal layers.
