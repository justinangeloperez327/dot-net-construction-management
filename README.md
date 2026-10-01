# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 24 — Staging

The application now includes:

- a multi-stage .NET 10 container image running as a non-root user
- persistent staging paths for uploads and Data Protection keys
- startup validation for non-development deployment configuration
- a dedicated `appsettings.Staging.json`
- container smoke testing against the liveness endpoint
- an EF Core migration bundle artifact for controlled schema updates
- a GitHub Actions staging pipeline that publishes commit and `staging` image tags to GHCR
- CI validation that the deployment container still builds

Staging intentionally does not auto-apply database migrations from application startup. Apply the generated migration bundle once before rolling out the matching image.

See `docs/staging.md`.

## Build

Requires the .NET 10 SDK.

dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build

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
