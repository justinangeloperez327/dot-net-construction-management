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

**Group 10 — Document Control**

The application now includes a project-scoped document register with:

- unique document number per project
- document title
- flexible category
- flexible discipline
- originator
- description
- Active / Archived lifecycle
- creator identity and registration timestamp
- project-scoped search and status filtering
- pagination
- permission-controlled create/update/archive/restore
- closed-project write protection
- SQL Server persistence
- Blazor document register and detail pages
- Domain, Application, and integration tests

Group 10 intentionally stores the **document master record only**.

Files are not attached directly to the document master. Group 11 will add document revisions, and each revision will own its file through the existing `IFileStorage` abstraction. This avoids migrating a single document-level file model into a revision model later.

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

See:
- `docs/daily-site-reports.md`
- `docs/daily-report-resources.md`
- `docs/file-storage-attachments.md`
- `docs/document-control.md`

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
