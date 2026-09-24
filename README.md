# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 14 — Purchase Requests

The application now supports project-scoped Purchase Requests with:

- project-scoped PR numbering
- title and purpose / justification
- optional required-by date
- requested-by identity and creation timestamp
- line items with description, quantity, unit, and remarks
- Draft / Pending Approval / Approved / Rejected / Cancelled lifecycle
- edit and resubmit after rejection
- ordered approval-chain selection
- Group 12 approval-engine integration
- automatic approval outcome synchronization
- project closure protection
- pagination and status filtering
- SQL Server persistence
- Blazor create/register/detail workflow

Purchase Requests intentionally do not select suppliers or establish final prices. They define what the project needs. Supplier selection and committed commercial terms belong to later procurement stages.

Group 14 also introduces a narrow IUnitOfWork because submission is the first workflow that must atomically persist changes across PurchaseRequest and ApprovalRequest.

## Build

Requires the .NET 10 SDK.

dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build

Apply database migrations explicitly with dotnet ef database update using Infrastructure as the migration project and Web as the startup project.

See docs/purchase-requests.md.

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
