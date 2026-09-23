# Persistence

## Provider

Persistence uses Entity Framework Core with SQL Server.

The Entity Framework Core implementation lives entirely in `Infrastructure`.

```text
Domain
  ↑
Application
  ↑
Infrastructure
  └── Persistence
      └── ApplicationDbContext
```

Neither Domain nor Application references Entity Framework Core.

## Connection strings

Development uses SQL Server LocalDB through `appsettings.Development.json`.

Production must supply the connection string through configuration. The recommended environment variable is:

```text
ConnectionStrings__Database
```

Do not commit production credentials or secrets to the repository.

## Migrations

The repository uses a local `dotnet-ef` tool manifest.

Restore tools:

```bash
dotnet tool restore
```

No empty initial migration is created in Group 2 because there is no persisted domain model yet. The first migration should be created together with the first real persisted aggregate.

When a persisted model exists, create a migration with:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/Web \
  --output-dir Persistence/Migrations
```

Run the command with the Development environment or provide `ConnectionStrings__Database`.

Apply migrations explicitly:

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web
```

Production startup must not automatically execute database migrations. Migrations are a deployment responsibility so schema changes can be reviewed and controlled.

## Health checks

Two endpoints are exposed:

- `/health/live` verifies that the web process is running.
- `/health/ready` includes the SQL Server database check.

Readiness requires an accessible configured database. CI currently verifies registration and the live endpoint without requiring a SQL Server instance.
