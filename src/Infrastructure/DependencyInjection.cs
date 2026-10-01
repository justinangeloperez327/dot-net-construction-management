using Application.Approvals;
using Application.Auditing;
using Application.Clients;
using Application.Common.Authorization;
using Application.Commercial;
using Application.Common.Files;
using Application.Common.Persistence;
using Application.DailyReports;
using Application.Dashboard;
using Application.Deliveries;
using Application.Documents;
using Application.PaymentApplications;
using Application.Projects;
using Application.Reporting;
using Application.Security;
using Application.PurchaseOrders;
using Application.PurchaseRequests;
using Application.Roles;
using Application.Suppliers;
using Application.Users;
using Infrastructure.Files;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' is not configured.");
        }

        var commandTimeoutSeconds = GetBoundedInt(
            configuration["Database:CommandTimeoutSeconds"],
            fallback: 30,
            minimum: 5,
            maximum: 300);
        var maxRetryCount = GetBoundedInt(
            configuration["Database:MaxRetryCount"],
            fallback: 5,
            minimum: 0,
            maximum: 10);
        var maxRetryDelaySeconds = GetBoundedInt(
            configuration["Database:MaxRetryDelaySeconds"],
            fallback: 10,
            minimum: 1,
            maximum: 60);

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.CommandTimeout(
                        commandTimeoutSeconds);
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount,
                        TimeSpan.FromSeconds(maxRetryDelaySeconds),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<ApplicationSignInManager>()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/access-denied";
            options.Cookie.Name = "__Host-ConstructionManagement.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
            options.Cookie.IsEssential = true;
            options.SlidingExpiration = false;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(4);
        });

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy => policy.AddRequirements(
                        new PermissionRequirement(permission)));
            }
        });

        services.AddSingleton<
            IAuthorizationHandler,
            PermissionAuthorizationHandler>();

        services.AddScoped<IUserAdministration, UserAdministration>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IRoleAdministration, RoleAdministration>();

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IDailyReportRepository, DailyReportRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseRequestRepository, PurchaseRequestRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IPaymentApplicationRepository, PaymentApplicationRepository>();
        services.AddScoped<ICommercialCertificationRepository, CommercialCertificationRepository>();
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();
        services.AddScoped<IReportingQueryService, ReportingQueryService>();
        services.AddScoped<IAuditTrailQueryService, AuditTrailQueryService>();
        services.AddScoped<ISecurityEventRecorder, SecurityEventStore>();
        services.AddScoped<ISecurityEventQueryService, SecurityEventStore>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        var storageRoot = configuration["FileStorage:RootPath"];

        if (string.IsNullOrWhiteSpace(storageRoot))
        {
            storageRoot = "App_Data/uploads";
        }

        if (!Path.IsPathRooted(storageRoot))
        {
            storageRoot = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    storageRoot));
        }

        services.AddSingleton(
            new FileStorageOptions(storageRoot));

        services.AddSingleton<IFileStorage, FileSystemFileStorage>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: ["ready"]);

        return services;
    }

    private static int GetBoundedInt(
        string? value,
        int fallback,
        int minimum,
        int maximum)
    {
        return int.TryParse(value, out var parsed)
            ? Math.Clamp(parsed, minimum, maximum)
            : fallback;
    }
}
