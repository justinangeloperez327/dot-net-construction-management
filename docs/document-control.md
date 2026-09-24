# Document control

Group 10 introduces the project document register.

## Master record

A `Document` is the stable identity for a controlled project document.

It records:

- project
- document number
- title
- category
- discipline
- originator
- description
- Active / Archived state
- creator
- registration timestamp

The document number is unique **within a project**, not globally.

This allows two separate projects to use the same client/consultant numbering convention without false conflicts.

## Flexible metadata

Category, discipline, and originator are stored as normalized text rather than hard-coded enums.

This is intentional. Construction organizations commonly use different:

- document type codes
- discipline codes
- consultant/main-contractor abbreviations
- client-specific naming conventions

A fixed taxonomy should only be introduced when the organization has approved values and governance rules.

## Lifecycle

```text
Active ↔ Archived
```

Archiving removes a document from the active register without deleting its identity.

Archived documents cannot be edited until restored.

Documents in a closed project cannot be:

- created
- updated
- archived
- restored

They remain readable.

## Files and revisions

A document master does **not** directly own a file in Group 10.

Group 11 will introduce:

```text
Document
  └── Revision
        └── File
```

The revision will reuse the `IFileStorage` abstraction created in Group 9.

This prevents an incorrect model where a controlled document can have only one binary file.

## Permissions

```text
documents.view
documents.create
documents.update
documents.archive
```

## Routes

```text
/projects/{projectId}/documents
/projects/{projectId}/documents/create
/documents/{documentId}
```

## Persistence

Table:

```text
Documents
```

Important constraints:

- primary key on `Id`
- unique `ProjectId + DocumentNumber`
- foreign key to `Projects`
- foreign key to the Identity user who registered the document
