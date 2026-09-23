# Projects

Group 5 introduces the first construction-business aggregate.

## Scope

A project currently contains:

- project number
- project name
- location
- start date
- target completion date
- description
- status

The status model is intentionally small:

```text
Active
Closed
```

More lifecycle states should be added only when an actual workflow requires them.

Clients and project members are intentionally excluded from Group 5 and belong to Group 6.

## Domain rules

- project number is required
- project name is required
- target completion date cannot be before start date
- new projects are Active
- closed projects cannot have their core details edited
- closing a project is idempotent

Project-number uniqueness is enforced by the application and by a unique database index.

## Clean Architecture flow

```text
Blazor pages
    ↓
Application handlers
    ↓
IProjectRepository
    ↑
ProjectRepository (Infrastructure)
    ↓
ApplicationDbContext / SQL Server

Application handlers
    ↓
Project aggregate (Domain)
```

Entity Framework Core remains in Infrastructure. Domain and Application do not depend on EF Core.

## Permissions

```text
projects.view
projects.create
projects.update
projects.close
```

The Administrator bootstrap automatically receives newly defined permissions.

## User interface

Routes:

```text
/projects
/projects/create
/projects/{id}
/projects/{id}/edit
```

The project list supports search, status filtering, and pagination.
