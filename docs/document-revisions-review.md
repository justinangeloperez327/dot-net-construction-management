# Document revisions and review

Group 11 adds immutable document revisions and review history to Document Control.

## Model

```text
Document
  └── DocumentRevision
        ├── revision code
        ├── change summary
        ├── immutable file metadata
        ├── creator
        ├── submission metadata
        └── review metadata
```

Revision codes are free-form. Examples include:

```text
00
01
A
B
P01
C01
```

The application does not assume one numbering convention.

## Immutability

A revision's identity, revision code, file, and change summary are created once.

There is intentionally no:

- replace-file command
- update-revision-code command
- delete-revision command

If a revision is rejected or requires a corrected file, create a new revision.

This preserves the historical record.

## Workflow

```text
Draft
  ↓
Submitted
  ├── Approved
  ├── Approved With Comments
  └── Rejected

Approved / Approved With Comments
  ↓ when a newer revision is approved
Superseded
```

Only one Draft or Submitted revision may exist for a document at a time.

Review comments are required for:

- Approved With Comments
- Rejected

Plain Approved may have optional comments.

## Current revision

The newest revision with status:

- Approved
- Approved With Comments

is treated as the current approved revision.

When a newer revision is approved, earlier approved revisions automatically become Superseded.

## Document lifecycle

An archived document cannot receive, submit, or review revisions.

A document cannot be archived while a Draft or Submitted revision exists.

A closed project blocks:

- new revisions
- submission
- review

Historical revisions and files remain readable.

## File storage

Revision files reuse `IFileStorage`.

Storage keys use:

```text
documents/{documentId}/revisions/{revisionId}.{extension}
```

The file itself remains outside SQL Server and outside `wwwroot`.

Group 11 uses the existing conservative upload allow-list:

- JPEG
- PNG
- WebP
- PDF
- maximum 25 MB

Additional controlled-document formats should only be enabled when their security and business requirements are defined.

## Permissions

```text
documents.view
documents.revisions.create
documents.revisions.submit
documents.revisions.review
```

Revision downloads use `documents.view`.

## Routes

```text
/documents/{documentId}/revisions
/files/documents/{documentId}/revisions/{revisionId}
```

## Persistence

Table:

```text
DocumentRevisions
```

Important constraints:

- primary key on `Id`
- unique `DocumentId + RevisionCode`
- unique storage key
- foreign key to Document
- creator / submitter / reviewer foreign keys to Identity users
