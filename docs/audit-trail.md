# Audit Trail

Group 21 adds an append-only audit trail for business-domain changes.

## Purpose

The audit trail answers:

- who changed a business record
- when the change occurred
- what entity was changed
- which record was affected
- which project the change belongs to when that context can be resolved
- whether the record was created, updated, or deleted
- which scalar properties changed

It is not an application log, security-event log, or workflow history replacement.

Existing approval/workflow entities remain the source of truth for business decisions.

## Capture model

Audit capture is implemented with an EF Core SaveChanges interceptor:

Infrastructure.Persistence.AuditSaveChangesInterceptor

The interceptor records Domain.* entities only.

This deliberately excludes ASP.NET Core Identity entities and therefore never serializes:

- password hashes
- security stamps
- authenticator keys
- login tokens
- role claims
- other Identity persistence internals

Security/authentication event auditing can be introduced explicitly during Group 22 Security Hardening using sanitized event models.

## Transaction consistency

Audit rows are added to the same DbContext before SaveChanges completes.

The business change and its audit records therefore participate in the same database transaction.

There is no separate asynchronous audit write.

## Audit record

Each AuditLog stores:

- Id
- ProjectId when resolvable
- ActorUserId
- ActorEmail
- Action
- EntityType
- EntityId
- ChangesJson
- OccurredAt

Actions:

- Created
- Updated
- Deleted

## Changed properties

ChangesJson contains scalar property changes.

For a create event:

old = null
new = created value

For an update event:

only modified properties are recorded

For a delete event:

old = deleted value
new = null

Enums are stored by name.

Dates/times use invariant textual representations.

Binary values are never serialized; they are represented by a size marker.

The audit trail records file metadata such as names/storage keys but never attachment file contents.

## Entity identity

Single-column keys are stored directly.

Composite keys are represented as:

PropertyA=value;PropertyB=value

This supports records such as ProjectMember that do not use a standalone Id.

## Project context

For aggregate roots that have ProjectId, the value is recorded directly.

For Project itself, its Id becomes ProjectId.

For child entities, the interceptor attempts to resolve ProjectId by following tracked aggregate relationships in the current DbContext.

It does not issue additional database queries from inside SaveChanges.

If a child entity is persisted independently and project context cannot be resolved from the tracked graph, ProjectId remains null. The audit event itself is still retained.

## Actor context

Async application writes use ICurrentUser.

Authenticated changes record:

- ActorUserId
- ActorEmail

When there is no authenticated actor, the UI displays System.

Synchronous persistence operations are still audited but do not block waiting for authentication context and are therefore treated as system changes.

## Immutability

AuditLogs are append-only.

AuditSaveChangesInterceptor rejects any tracked AuditLog entry in:

- Modified
- Deleted

No application service exists to update or delete audit rows.

The AuditLogs table intentionally has no foreign keys to Projects, Users, or business entities. Historical evidence therefore remains available even if the referenced business record is later removed.

## Query indexes

Indexes are provided for:

- OccurredAt
- ProjectId + OccurredAt
- ActorUserId + OccurredAt
- EntityType + EntityId

## Authorization

Permission:

audit_trail.view

The bootstrap Administrator receives it through Permissions.All synchronization.

## Routes

Global history:

/audit-trail

Project-filtered history:

/projects/{projectId}/audit-trail

## Filters

The UI supports:

- Project
- Action
- Entity Type
- Actor email
- From Date
- To Date

Results are paginated at 50 rows by default with a maximum page size of 100.

Property changes are hidden behind an expandable details section so the main timeline remains readable.

## Persistence

New table:

AuditLogs

Migration:

AddAuditTrail

## Scope boundary

Group 21 deliberately does not add:

- request/response logging
- IP-address tracking
- browser/device fingerprinting
- password/authentication audit payloads
- log shipping
- SIEM integration
- audit retention/purge policy
- tamper-evident hashing
- cryptographic signing
- audit export

Security-event logging and retention controls belong to Group 22 Security Hardening and later operational work.
