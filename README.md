# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 18 — Commercial Approvals

The application now supports formal commercial certification of approved Payment Applications.

Commercial Certification includes:

- one certificate per approved Payment Application
- project-scoped certificate number
- claimed amount snapshot
- commercial certified amount
- uncertified amount
- retention percentage and derived retention amount
- advance-payment recovery
- reasoned other deductions
- total deductions
- derived payable amount
- Draft / Pending Approval / Approved / Rejected / Cancelled lifecycle
- reusable approval-engine integration
- project certification register
- certification workflow from approved Payment Applications
- SQL Server persistence

The core formula is:

Payable Amount = Certified Amount - Retention - Advance Recovery - Other Deductions

Certified Amount cannot exceed the supplier's approved claim, and total deductions cannot exceed the certified amount.

See docs/commercial-certifications.md.

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
