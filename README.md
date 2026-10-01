# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 16 — Deliveries

The application now supports partial delivery receiving against approved Purchase Orders.

Deliveries include:

- supplier delivery note number
- delivery date
- optional vehicle / transport reference
- remarks
- creator and creation timestamp
- Draft / Received / Cancelled lifecycle
- selected Purchase Order lines
- actual delivered quantities
- cumulative received-quantity protection
- receiving user and timestamp
- immutable posted receipts
- project delivery register
- approved-PO delivery creation workflow
- SQL Server persistence

Multiple deliveries can fulfill one PO line. Only Received deliveries consume the ordered quantity. Draft receipts do not reserve quantity.

Once a delivery is Received it is immutable in Group 16. Reversals, returns, rejected materials, warehouse stock, quality inspections, and invoice matching require explicit later workflows and are not simulated by editing historical receipts.

## Build

Requires the .NET 10 SDK.

dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build

Apply database migrations explicitly with dotnet ef database update using Infrastructure as the migration project and Web as the startup project.

See docs/deliveries.md.

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
