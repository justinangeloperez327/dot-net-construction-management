# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Current milestone

Group 22 — Security Hardening

The application now includes:

- five-minute Interactive Server authentication revalidation
- four-minute Identity security-stamp cookie validation
- stricter password and lockout policy
- secure host-only authentication cookie
- CSP and defensive HTTP headers
- HSTS
- trusted forwarded-header handling
- non-wildcard host filtering
- request/form size limits
- rate limiting on authentication, export, and file-download endpoints
- persistent Data Protection key configuration
- sanitized security-event storage
- upload magic-byte validation for JPEG, PNG, WebP, and PDF

Security events are available at /security-events.

AllowedHosts is restricted to localhost by default. Production must explicitly configure its host names and trusted proxies.

See docs/security-hardening.md.

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
