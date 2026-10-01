# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 19 — Dashboard

The application now provides live portfolio and project dashboards over the existing construction-management workflows.

Portfolio dashboard:

- active / closed / overdue projects
- open site issues
- the signed-in user's pending approval steps
- Purchase Requests pending approval
- Purchase Orders pending approval
- draft Deliveries
- Payment Applications pending approval
- Commercial Certifications pending approval
- active-project attention cards

Project dashboard:

- target completion and overdue state
- latest Daily Report
- open site issues
- pending project approvals
- Daily Report status counts
- Document status counts
- Purchase Request status counts
- Purchase Order status counts
- Delivery status counts
- Payment Application status counts
- Commercial Certification status counts

The dashboard uses a dedicated EF Core read-model service instead of forcing analytical queries through transactional repositories.

Group 19 adds no database tables or migration.

See docs/dashboard.md.

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
