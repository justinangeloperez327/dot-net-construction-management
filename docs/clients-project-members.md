# Clients and project members

Group 6 adds project relationships without duplicating identity data.

## Clients

A Client is a separate Domain aggregate.

Current fields:

- name
- contact person
- email
- phone
- address
- active/inactive status

A project may have zero or one client.

Only active clients may be newly assigned to a project. Deactivating a client does not erase historical project relationships.

## Project members

A ProjectMember represents an assignment:

```text
ProjectId + UserId + Responsibility
```

The composite `ProjectId + UserId` key prevents assigning the same user to one project more than once.

The Domain stores only the existing application user's `Guid`. It does not reference ASP.NET Core Identity types.

Infrastructure maps `ProjectMember.UserId` to the Identity users table.

This avoids creating a second employee/user record merely for project membership.

## Responsibility

Responsibility/title is currently free text, for example:

```text
Project Manager
Project Coordinator
Site Engineer
Quantity Surveyor
```

It is intentionally not an enum yet because the organization's controlled project-role vocabulary has not been established.

## Closed projects

Closed projects cannot:

- change their assigned client
- add members
- change member responsibility
- remove members

This keeps closed project composition stable.

## Permissions

```text
clients.view
clients.create
clients.update
clients.set_active
projects.manage_members
```

Client assignment uses the existing `projects.update` permission.

## Routes

```text
/clients
/clients/create
/clients/{id}

/projects/{id}/client
/projects/{id}/members
```
