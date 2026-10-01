# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 21 — Audit Trail

The application now records append-only business-data audit events through an EF Core SaveChanges interceptor.

Audit events capture:

- create / update / delete operation
- entity type
- entity identifier
- project context when resolvable
- authenticated actor ID and email
- UTC occurrence timestamp
- JSON scalar-property changes

Only Domain entities are audited. ASP.NET Core Identity persistence is deliberately excluded so password hashes, security stamps, authentication tokens, and role-claim internals are never copied into audit JSON.

Audit records are inserted in the same database transaction as the business change.

AuditLogs are append-only at the application persistence layer and have no foreign keys back to business records, preserving history independently from later record deletion.

Routes:

- /audit-trail
- /projects/{projectId}/audit-trail

Filters:

- Project
- Action
- Entity Type
- Actor
- From Date
- To Date

See docs/audit-trail.md.

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
