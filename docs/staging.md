# Staging

Group 24 defines a repeatable staging release path without binding the application to a specific cloud provider.

## Release artifacts

Each successful staging workflow run produces:

- `ghcr.io/<owner>/<repository>:<commit-sha>`
- `ghcr.io/<owner>/<repository>:staging`
- a self-contained Linux x64 EF Core migration bundle named `efbundle`
- `staging-release.json` containing the commit and image tags

The immutable commit tag is the deployment source of truth. The `staging` tag is a convenience channel.

## Required staging configuration

Set these as environment variables in the staging runtime:

```text
ASPNETCORE_ENVIRONMENT=Staging
ConnectionStrings__Database=<staging SQL Server connection string>
AllowedHosts=<staging hostname>
```

The container defaults persistent application data to:

```text
/app/data/uploads
/app/data/keys
```

Mount `/app/data` on durable storage. Uploads and Data Protection keys must survive container replacement.

If a reverse proxy is used, also configure trusted proxy IP addresses with indexed environment variables such as:

```text
Security__ForwardedHeaders__KnownProxies__0=10.0.0.10
```

Do not use a wildcard `AllowedHosts` value.

## Database migration

Database migration is deliberately separated from application startup.

Before deploying a new staging image:

1. Download the `staging-release-<commit>` artifact from the matching workflow run.
2. Run `efbundle --connection "<staging connection string>"` exactly once against the staging database.
3. Deploy the image with the same commit SHA.
4. Verify `/health/live`.
5. Verify `/health/ready` after the database is reachable.

This avoids multi-instance migration races and keeps schema changes explicit.

## Bootstrap administrator

For an empty staging database, a bootstrap administrator can be created by setting both values for the first controlled deployment:

```text
BootstrapAdmin__Email=<administrator email>
BootstrapAdmin__Password=<strong temporary password>
```

Remove these values after the account has been created.

## Container runtime

The image:

- uses the official .NET 10 SDK only during build
- runs on the ASP.NET Core runtime image
- listens on port 8080
- runs as the image-provided non-root application user
- disables runtime diagnostics in the container
- stores mutable application data under `/app/data`

## GitHub staging environment

The workflow targets the GitHub environment named `staging`. Repository administrators can add environment protection rules before connecting a real staging host.

The workflow publishes the validated image and migration artifact only. Provider-specific rollout credentials are intentionally deferred until the staging hosting platform is selected.
