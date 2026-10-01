# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 15 — Purchase Orders

The application now supports commercial Purchase Orders created from approved Purchase Requests.

Purchase Orders include:

- company-wide unique PO number
- approved source Purchase Request
- active Supplier
- order and expected-delivery dates
- three-letter currency code
- delivery address and delivery terms
- payment terms
- notes
- source-linked PO lines
- quantities
- unit prices
- line discounts
- line taxes
- derived commercial totals
- Draft / Pending Approval / Approved / Rejected / Cancelled lifecycle
- reusable Group 12 approval integration
- split-award quantity controls
- project closure protection
- project PO register and detail workflow
- SQL Server persistence
- Domain and integration tests

A source PR may create more than one PO. At submission, quantities already present in Pending Approval or Approved POs are counted so combined supplier awards cannot exceed the approved PR line quantity.

## Build

Requires the .NET 10 SDK.

dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build

Apply database migrations explicitly with dotnet ef database update using Infrastructure as the migration project and Web as the startup project.

See docs/purchase-orders.md.

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
