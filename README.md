# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 23 — Performance and Database Optimization

The application now includes:

- consolidated portfolio project metrics to reduce database round trips
- composite indexes aligned with dashboard, workflow, and reporting hot paths
- optimized project/date/status access patterns
- configurable SQL Server command timeout and transient retry behavior
- HTTPS response compression, including CSV exports
- EF Core model coverage tests for performance-critical indexes

Database tuning defaults are configured under `Database` in `appsettings.json`.

See `docs/performance-database-optimization.md`.

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
