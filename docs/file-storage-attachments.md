# File storage and Daily Report attachments

Group 9 introduces a reusable file-storage boundary and attaches files to Daily Site Reports.

## Architecture

```text
Web / Blazor
    ↓
Application attachment handlers
    ↓
IFileStorage
    ↑
FileSystemFileStorage (Infrastructure)

DailyReport
    └── DailyReportAttachment metadata
            └── StorageKey → external file bytes
```

The Domain does not know about folders, disks, Azure, HTTP, or Blazor.

Document Control can reuse `IFileStorage` later while owning its own document/revision metadata.

## Storage rules

The default provider writes under:

```text
App_Data/uploads
```

The root is configurable using:

```text
FileStorage__RootPath
```

Relative paths are resolved from the application base directory.

The provider:

- rejects absolute storage keys
- rejects keys that escape the configured root
- creates files with `CreateNew` so an existing object is never silently overwritten
- keeps files outside `wwwroot`
- supports streamed reads and writes

For production deployment, configure `FileStorage__RootPath` to durable storage. The `IFileStorage` boundary allows replacing the filesystem implementation with Azure Blob Storage without changing Domain or Application code.

## Daily Report attachment metadata

SQL Server stores:

- attachment ID
- Daily Report ID
- generated storage key
- original file name
- content type
- byte size
- uploader user ID
- upload timestamp
- optional caption

File bytes are not stored in SQL Server.

## Upload policy

Current allowed types:

```text
JPEG
PNG
WebP
PDF
```

Maximum file size:

```text
25 MB
```

SVG and executable formats are intentionally excluded.

File names are reduced to their base name before persistence, and the file extension must agree with the declared MIME type.

Content malware scanning and deeper file-signature inspection belong to the Security Hardening milestone.

## Workflow

Attachments are part of the DailyReport aggregate.

```text
Draft      -> upload/delete allowed
Rejected   -> upload/delete allowed
Submitted  -> locked
Approved   -> locked
```

Downloads remain available to authorized users regardless of report workflow state.

## Authorization

Group 9 reuses:

```text
daily_reports.view
daily_reports.update
```

No separate attachment permission is introduced because attachments are report content.

## Routes

```text
/daily-reports/{reportId}/attachments

/files/daily-reports/{reportId}/attachments/{attachmentId}
```

The file endpoint requires `daily_reports.view` and returns the original file name as a download.
