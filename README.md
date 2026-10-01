# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 20 — Reporting

The application now provides historical, filterable project reporting and CSV exports.

Report workspace:

- Project Portfolio
- Daily Site Register
- Procurement Register
- Commercial Register

Filters:

- Project
- From Date
- To Date

CSV exports:

- use the same reporting read models as the UI
- export the complete filtered dataset
- use UTF-8 BOM for Excel compatibility
- use invariant dates and decimals
- correctly escape commas, quotes, and line breaks

On-screen report tables are limited to 200 rows to avoid unbounded Blazor rendering. When more rows exist, the UI directs the user to the full CSV export.

Reporting uses a dedicated Application read contract and EF Core read service. No reporting state is persisted and Group 20 adds no database migration.

See docs/reporting.md.

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
