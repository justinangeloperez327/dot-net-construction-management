# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Architecture

The solution has four production projects:

- **Domain** — business rules and domain model.
- **Application** — use cases and application abstractions.
- **Infrastructure** — persistence, identity, files, and integrations.
- **Web** — ASP.NET Core + Blazor presentation and composition root.

Dependencies point inward.

## Current milestone

**Group 7 — Daily Site Reports**

The application now supports:

- one daily report per project/date
- preparer identity
- weather and general remarks
- multiple site activity rows
- location/activity/status/progress/remarks
- Draft → Submitted → Approved/Rejected workflow
- rejected report correction and resubmission
- reviewer identity and comments
- search/filter-free project-scoped report history
- permission-controlled Blazor reporting UI
- Domain, Application, and integration tests

Manpower, equipment, and site issues remain Group 8. Attachments remain Group 9.

## Build

Requires the .NET 10 SDK.

```bash
dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build
```

Apply database migrations explicitly:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

See `docs/daily-site-reports.md`.

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
