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

    public static class Suppliers
    {
        public const string View = "suppliers.view";
        public const string Create = "suppliers.create";
        public const string Update = "suppliers.update";
        public const string SetActive = "suppliers.set_active";
    }

    public static class PurchaseRequests
    {
        public const string View = "purchase_requests.view";
        public const string Create = "purchase_requests.create";
        public const string Update = "purchase_requests.update";
        public const string Submit = "purchase_requests.submit";
        public const string Cancel = "purchase_requests.cancel";
    }

    public static class PurchaseOrders
    {
        public const string View = "purchase_orders.view";
        public const string Create = "purchase_orders.create";
        public const string Update = "purchase_orders.update";
        public const string Submit = "purchase_orders.submit";
        public const string Cancel = "purchase_orders.cancel";
    }

    public static class Deliveries
    {
        public const string View = "deliveries.view";
        public const string Create = "deliveries.create";
        public const string Update = "deliveries.update";
        public const string Receive = "deliveries.receive";
        public const string Cancel = "deliveries.cancel";
    }

    public static class PaymentApplications
    {
        public const string View = "payment_applications.view";
        public const string Create = "payment_applications.create";
        public const string Update = "payment_applications.update";
        public const string Submit = "payment_applications.submit";
        public const string Cancel = "payment_applications.cancel";
    }

    public static class CommercialCertifications
    {
        public const string View = "commercial_certifications.view";
        public const string Create = "commercial_certifications.create";
        public const string Update = "commercial_certifications.update";
        public const string Submit = "commercial_certifications.submit";
        public const string Cancel = "commercial_certifications.cancel";
    }

    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    public static class Reports
    {
        public const string View = "reports.view";
        public const string Export = "reports.export";
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

    public static class Documents
    {
        public const string View = "documents.view";
        public const string Create = "documents.create";
        public const string Update = "documents.update";
        public const string Archive = "documents.archive";
        public const string CreateRevision = "documents.revisions.create";
        public const string SubmitRevision = "documents.revisions.submit";
        public const string ReviewRevision = "documents.revisions.review";
    }

    public static class Approvals
    {
        public const string View = "approvals.view";
        public const string Create = "approvals.create";
        public const string Decide = "approvals.decide";
        public const string Cancel = "approvals.cancel";
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
        Suppliers.View,
        Suppliers.Create,
        Suppliers.Update,
        Suppliers.SetActive,
        PurchaseRequests.View,
        PurchaseRequests.Create,
        PurchaseRequests.Update,
        PurchaseRequests.Submit,
        PurchaseRequests.Cancel,
        PurchaseOrders.View,
        PurchaseOrders.Create,
        PurchaseOrders.Update,
        PurchaseOrders.Submit,
        PurchaseOrders.Cancel,
        Deliveries.View,
        Deliveries.Create,
        Deliveries.Update,
        Deliveries.Receive,
        Deliveries.Cancel,
        PaymentApplications.View,
        PaymentApplications.Create,
        PaymentApplications.Update,
        PaymentApplications.Submit,
        PaymentApplications.Cancel,
        CommercialCertifications.View,
        CommercialCertifications.Create,
        CommercialCertifications.Update,
        CommercialCertifications.Submit,
        CommercialCertifications.Cancel,
        Dashboard.View,
        Reports.View,
        Reports.Export,
        Projects.View,
        Projects.Create,
        Projects.Update,
        Projects.Close,
        Projects.ManageMembers,
        DailyReports.View,
        DailyReports.Create,
        DailyReports.Update,
        DailyReports.Submit,
        DailyReports.Review,
        Documents.View,
        Documents.Create,
        Documents.Update,
        Documents.Archive,
        Documents.CreateRevision,
        Documents.SubmitRevision,
        Documents.ReviewRevision,
        Approvals.View,
        Approvals.Create,
        Approvals.Decide,
        Approvals.Cancel
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
