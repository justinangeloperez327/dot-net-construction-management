# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 25 — Production Deployment

The application now includes:

- a manual, versioned production release workflow
- GitHub `production` environment gating for release approvals
- immutable commit and versioned GHCR image tags
- an optional `latest` production image tag
- production-mode container smoke testing before publication
- a self-contained Linux x64 EF Core migration bundle for controlled schema updates
- production startup validation for database, host, and persistent storage configuration
- a dedicated `appsettings.Production.json`
- GitHub Release creation with the migration bundle and release manifest
- documented deployment, verification, and rollback procedures

Production releases are deliberately manual. The release workflow must be dispatched from `main` with an explicit version such as `v1.0.0`.

See `docs/production-deployment.md`.

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
