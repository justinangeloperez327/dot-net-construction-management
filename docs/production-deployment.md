# Production Deployment

Group 25 defines the production release boundary for Construction Project Management.

The application remains hosting-provider agnostic. The repository produces a validated production image and migration artifact; the final runtime can be Azure, another container platform, or an internal container host without changing the application release contract.

## Production release policy

Production releases are manual.

Run the **Production Release** workflow from the `main` branch and provide an explicit semantic version:

```text
v1.0.0
v1.0.1
v1.1.0-rc.1
```

The workflow targets the GitHub environment named `production`. Configure required reviewers or other protection rules on that environment before the first live release.

A release cannot reuse an existing Git tag.

## Release gates

A production release must pass all of the following before images are published:

1. dependency restore
2. Release build
3. full automated test suite
4. EF Core pending-model verification
5. self-contained Linux x64 migration bundle generation
6. production container build
7. production-mode liveness smoke test

Only after those checks succeed does the workflow authenticate to GHCR and publish production images.

## Release outputs

For version `v1.0.0` at commit `<sha>`, the workflow publishes:

```text
ghcr.io/<owner>/<repository>:<sha>
ghcr.io/<owner>/<repository>:v1.0.0
ghcr.io/<owner>/<repository>:latest
```

Publishing `latest` can be disabled when dispatching the workflow.

The workflow also produces:

- `efbundle` — self-contained Linux x64 EF Core migration bundle
- `production-release.json` — immutable release manifest
- a GitHub Release containing both files

Use the versioned or commit-SHA image for deployment automation. Do not use `latest` as the production source of truth.

## Required runtime configuration

Set these values in the production runtime:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Database=<production SQL Server connection string>
AllowedHosts=<production hostname>
```

The container expects persistent application data at:

```text
/app/data/uploads
/app/data/keys
```

Mount `/app/data` on durable storage so uploaded files and Data Protection keys survive instance replacement.

When a reverse proxy is used, configure only trusted proxy addresses, for example:

```text
Security__ForwardedHeaders__KnownProxies__0=10.0.0.10
```

Production startup rejects wildcard or localhost-only host configuration.

## Database deployment

The web application never applies EF Core migrations automatically during startup.

For each release:

1. back up the production database according to the hosting platform's backup procedure
2. download `efbundle` from the matching GitHub Release
3. run `efbundle --connection "<production connection string>"` exactly once
4. deploy the image with the matching release version or commit SHA
5. verify `/health/live`
6. verify `/health/ready`
7. perform a short authenticated application smoke test

Separating migrations from application startup prevents multiple replicas from racing to update the schema.

## First production administrator

For a brand-new database, the existing bootstrap mechanism can create the first administrator by temporarily configuring:

```text
BootstrapAdmin__Email=<administrator email>
BootstrapAdmin__Password=<strong temporary password>
```

Remove both values after the administrator account is created. Do not keep the bootstrap password as a permanent production secret.

## Rollback

Application rollback is image-based.

If a release must be rolled back:

1. stop further rollout
2. redeploy the previous known-good versioned or commit-SHA image
3. verify `/health/live` and `/health/ready`
4. restore the database only when the new migration is incompatible with the previous application version and a tested restore is required

Do not blindly run EF migrations backward in production. Database rollback should follow a release-specific recovery plan because destructive down-migrations can lose data.

## Operational checks

After each production release, verify:

- liveness endpoint
- readiness endpoint
- sign-in
- dashboard
- a representative project read
- file download
- report export
- application logs for startup or database errors

Monitor security events and application logs during the initial rollout window.

## Hosting provider integration

The production workflow intentionally stops after publishing the validated release package.

Once a production hosting platform is selected, its deployment step should consume the immutable `:<version>` or `:<commit-sha>` GHCR image. Provider credentials should be stored only in the protected GitHub `production` environment.
