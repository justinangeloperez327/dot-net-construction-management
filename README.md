# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Architecture

The solution has four production projects:

- **Domain** — business rules and domain model. Depends on nothing outside the .NET base class library.
- **Application** — use cases and application abstractions. Depends on Domain.
- **Infrastructure** — external concerns such as persistence, identity, files, and integrations. Depends inward on Application and Domain.
- **Web** — ASP.NET Core + Blazor presentation and composition root.

Dependencies point inward:

```text
Web ------------> Application ---> Domain
  \                  ^
   \                 |
    ----> Infrastructure
```

Infrastructure may implement interfaces owned by Application. Domain must not depend on Application, Infrastructure, or Web.

## Current milestone

**Group 1 — Foundation**

- .NET 10 / C# 14 baseline
- Clean Architecture project boundaries
- Blazor Web App with Interactive Server enabled
- dependency injection entry points
- architecture dependency tests
- web integration smoke test
- GitHub Actions build and test pipeline

Persistence, SQL Server, authentication, and business features are intentionally deferred to subsequent groups so they are introduced only when their requirements are defined.

## Build

Requires the .NET 10 SDK.

```bash
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build
```

Run the web application:

```bash
dotnet run --project src/Web/Web.csproj
```

## Planned development order

1. Foundation
2. SQL Server + Entity Framework Core persistence
3. Authentication
4. Users, roles, and permissions
5. Projects
6. Clients and project members
7. Daily site reports
8. Manpower, equipment, and site issues
9. File storage and attachments
10. Document control
11. Document revisions and review
12. Approval workflow
13. Suppliers
14. Purchase requests
15. Purchase orders
16. Deliveries
17. Payment applications
18. Commercial approvals
19. Dashboard
20. Reporting
21. Audit trail
22. Security hardening
23. Performance and database optimization
24. Staging
25. Production deployment
