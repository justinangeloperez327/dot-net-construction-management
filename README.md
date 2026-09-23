# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Architecture

The solution has four production projects:

- **Domain** — business rules and domain model. Depends on nothing outside the .NET base class library.
- **Application** — use cases and application abstractions. Depends on Domain.
- **Infrastructure** — persistence, identity, files, and integrations. Depends inward on Application and Domain.
- **Web** — ASP.NET Core + Blazor presentation and composition root.

Dependencies point inward:

```text
Web ------------> Application ---> Domain
  \                  ^
   \                 |
    ----> Infrastructure
```

## Current milestone

**Group 5 — Projects**

The first construction-business vertical slice now includes:

- Project domain aggregate
- project creation
- project editing
- project closing
- project details
- searchable/filterable/paginated project list
- SQL Server persistence
- project-number uniqueness
- project permissions
- Blazor project-management pages
- Domain, Application, and integration tests

Clients and project members remain intentionally deferred to Group 6.

## Build

Requires the .NET 10 SDK.

```bash
dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build
```

Run:

```bash
dotnet run --project src/Web/Web.csproj
```

Apply database migrations explicitly:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

See `docs/projects.md` for the Project aggregate and use-case boundaries.

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
