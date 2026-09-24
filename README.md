# Construction Project Management

A deployable construction project management application built with .NET 10 and Blazor using Clean Architecture.

## Architecture

The solution has four production projects:

- **Domain** — business rules and domain model.
- **Application** — use cases and application abstractions.
- **Infrastructure** — persistence, identity, files, and integrations.
- **Web** — ASP.NET Core + Blazor presentation and composition root.

Dependencies point inward.

## Current milestone

**Group 12 — Approval Workflow**

The application now has a reusable sequential approval engine for upcoming procurement and commercial workflows.

It supports:

- generic approval requests linked to a subject type + subject ID
- optional project context
- immutable ordered approval steps
- explicitly assigned approvers
- one active step at a time
- approve / reject decisions
- mandatory rejection comments
- automatic activation of the next step
- automatic request completion after the final approval
- requester cancellation
- participant-scoped approval inbox
- pending-decision visibility
- SQL Server persistence
- Blazor approval inbox and detail UI
- Domain, Application, and integration tests

Group 12 intentionally does **not** add workflow templates, parallel approvals, delegation, escalation timers, or a visual workflow designer. Those should be introduced only when real Purchase Request, Purchase Order, and commercial workflows require them.

Daily Reports and Document Revisions keep their existing domain-specific review state. They are not rewritten onto this generic engine.

## Build

Requires the .NET 10 SDK.

```bash
dotnet tool restore
dotnet restore CPM.slnx
dotnet build CPM.slnx --configuration Release --no-restore
dotnet test CPM.slnx --configuration Release --no-build
```

Apply database migrations explicitly:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

See `docs/approval-workflow.md`.

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
