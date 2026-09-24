namespace Application.Common.Authorization;

public static class Permissions
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string SetActive = "users.set_active";
        public const string ManageRoles = "users.manage_roles";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string ManagePermissions = "roles.manage_permissions";
    }

    public static class Clients
    {
        public const string View = "clients.view";
        public const string Create = "clients.create";
        public const string Update = "clients.update";
        public const string SetActive = "clients.set_active";
    }

    public static class Projects
    {
        public const string View = "projects.view";
        public const string Create = "projects.create";
        public const string Update = "projects.update";
        public const string Close = "projects.close";
        public const string ManageMembers = "projects.manage_members";
    }

    public static class DailyReports
    {
        public const string View = "daily_reports.view";
        public const string Create = "daily_reports.create";
        public const string Update = "daily_reports.update";
        public const string Submit = "daily_reports.submit";
        public const string Review = "daily_reports.review";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Users.View,
        Users.Create,
        Users.Update,
        Users.SetActive,
        Users.ManageRoles,
        Roles.View,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Roles.ManagePermissions,
        Clients.View,
        Clients.Create,
        Clients.Update,
        Clients.SetActive,
        Projects.View,
        Projects.Create,
        Projects.Update,
        Projects.Close,
        Projects.ManageMembers,
        DailyReports.View,
        DailyReports.Create,
        DailyReports.Update,
        DailyReports.Submit,
        DailyReports.Review
    ];
}

public static class PermissionClaimTypes
{
    public const string Permission = "permission";
}

public static class SystemRoles
{
    public const string Administrator = "Administrator";
}
