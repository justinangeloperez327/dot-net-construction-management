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
- always receives every defined permission through bootstrap
- is intended for the initial system administrator and trusted administrators

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
2. ensures all known permissions exist as role claims
3. creates the configured user if it does not exist
4. ensures the user is active
5. assigns the Administrator role

The password is used only when the user must be created. It is not reset on every startup.

After successful bootstrap, remove the bootstrap password from the runtime configuration.

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
- administrative implementations
- bootstrap behavior

`Web` owns:

- Blazor administration pages
- navigation
- route authorization
