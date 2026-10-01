# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 17 — Payment Applications

The application now supports supplier payment applications against approved Purchase Orders and physically received quantities.

Payment Applications include:

- PO-scoped application number
- application date
- optional claim period
- notes
- creator and creation timestamp
- Draft / Pending Approval / Approved / Rejected / Cancelled lifecycle
- claim lines tied to PO items
- received quantity visibility
- prior committed claim visibility
- remaining claimable quantity
- PO unit price, discount, and tax terms
- derived claimed amount
- reusable approval workflow
- project payment-application register
- approved-PO creation workflow
- SQL Server persistence

The central control is:

received quantity - quantities in Pending Approval / Approved applications = claimable quantity

Draft and Rejected applications do not reserve claim quantity.

Payment applications remain available during project close-out because commercial settlement can continue after operational project closure.

## Build

Requires the .NET 10 SDK.

dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build

Apply database migrations explicitly with dotnet ef database update using Infrastructure as the migration project and Web as the startup project.

See docs/payment-applications.md.

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
