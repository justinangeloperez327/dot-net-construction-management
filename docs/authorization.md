# Users, roles, and permissions

## Model

ASP.NET Core Identity remains the account and role store.

Authorization is permission-based:

```text
User
  ↓
Role
  ↓
Permission claim
  ↓
Authorization policy
```

Pages and future application capabilities authorize against stable permission names instead of hard-coded role names.

Examples:

```text
users.view
users.create
users.update
users.set_active
users.manage_roles

roles.view
roles.create
roles.update
roles.delete
roles.manage_permissions
```

## Administrator role

`Administrator` is a reserved system role.

It:

- cannot be renamed
- cannot be deleted
- satisfies every permission policy by role
- is intended for the initial system administrator and trusted administrators

Normal roles receive access through explicit permission claims. Administrator access does not depend on synchronizing every future permission claim into the database.

The administration service prevents deactivating an Administrator or removing the Administrator role when that action would leave the system without any other active Administrator account.

## Bootstrap administrator

There is no public or self-registration flow.

To create the first administrator, provide both:

```text
BootstrapAdmin__Email
BootstrapAdmin__Password
```

Apply database migrations before starting the application with these settings.

On startup, when both values are configured, the application:

1. creates the Administrator role if it does not exist
2. seeds the currently known permission claims
3. creates the configured user if it does not exist
4. ensures the user is active
5. assigns the Administrator role

The password is used only when the user must be created. It is not reset on every startup.

After successful bootstrap, remove the bootstrap password from runtime configuration. Future permissions are still effective for Administrator users because the authorization handler treats the Administrator role as having all permissions.

## Deactivation

Users have an explicit `IsActive` state.

Deactivation is separate from temporary failed-login lockout. The custom sign-in manager rejects inactive accounts, and changing active state updates the security stamp.

## Boundaries

`Application` owns:

- permission names
- user/role administration interfaces
- administration request/result models

`Infrastructure` owns:

- ASP.NET Core Identity
- user and role persistence
- permission claims
- permission-policy enforcement
- administrative implementations
- bootstrap behavior

`Web` owns:

- Blazor administration pages
- navigation
- route authorization
