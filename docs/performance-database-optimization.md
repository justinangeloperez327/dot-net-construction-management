# Performance and Database Optimization

Group 23 improves hot read paths without changing domain behavior or Clean Architecture boundaries.

## Database query improvements

- Portfolio project metrics use one aggregate query instead of three database round trips.
- Existing read models continue to use `AsNoTracking()`.
- Multi-collection daily-report loading continues to use split-query execution.
- SQL Server command timeout and transient retry behavior are configurable.

Default settings:

```json
"Database": {
  "CommandTimeoutSeconds": 30,
  "MaxRetryCount": 5,
  "MaxRetryDelaySeconds": 10
}
```

Runtime values are bounded before being applied.

## Hot-path indexes

The Group 23 migration adds composite indexes for:

- project status and target-date dashboard queries
- daily report project/status/date queries
- open site issue lookups
- document project/status lists
- pending approval requests and assigned approval steps
- purchase request and purchase order workflow status
- purchase order project/date reporting
- delivery workflow and received-delivery reporting
- payment application workflow and date reporting
- commercial certification workflow status

The indexes are aligned with existing filters and sort keys instead of indexing every searchable column.

## HTTP response compression

HTTPS response compression is enabled for normal framework MIME types plus CSV exports. The middleware runs before static-file handling.

## Validation

CI checks that the EF Core model matches the migration snapshot. Integration tests also assert the presence of the performance-critical indexes.
