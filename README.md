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

**Group 9 — File Storage and Attachments**

Daily Site Reports now support private attachments with:

- reusable `IFileStorage` Application boundary
- filesystem storage implementation in Infrastructure
- attachment metadata in SQL Server
- generated opaque storage keys
- authenticated upload/download/delete workflows
- JPEG, PNG, WebP, and PDF allow-list
- 25 MB per-file limit
- optional captions
- uploader and upload timestamp
- Draft/Rejected attachment editing
- Submitted/Approved attachment locking
- storage path traversal protection
- download responses forced through an authorized application endpoint

File bytes are not stored in SQL Server and are not exposed under `wwwroot`.

The storage root is configurable through:

```text
FileStorage__RootPath
```

The default is `App_Data/uploads` under the application base directory.

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
