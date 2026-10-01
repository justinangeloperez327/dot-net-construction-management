using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Application;
using Application.Common.Authentication;
using Application.Common.Authorization;
using Application.Common.Files;
using Application.DailyReports;
using Application.Documents;
using Application.Reporting;
using Application.Security;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Web.Authentication;
using Web.Components;
using Web.Security;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        FileUploadPolicy.MaxFileSizeBytes + (1024 * 1024);
    options.Limits.RequestHeadersTimeout =
        TimeSpan.FromSeconds(15);
});

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(["text/csv"]);
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<
    AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = 64;
    options.KeyLengthLimit = 256;
    options.ValueLengthLimit = 16 * 1024;
    options.MultipartBodyLengthLimit =
        FileUploadPolicy.MaxFileSizeBytes + (1024 * 1024);
    options.MultipartHeadersLengthLimit = 16 * 1024;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 1;

    var knownProxies = builder.Configuration
        .GetSection("Security:ForwardedHeaders:KnownProxies")
        .Get<string[]>()
        ?? [];

    foreach (var configuredProxy in knownProxies)
    {
        if (IPAddress.TryParse(
                configuredProxy,
                out var proxyAddress))
        {
            options.KnownProxies.Add(proxyAddress);
        }
    }

    var allowedHosts = builder.Configuration["AllowedHosts"]?
        .Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries)
        ?? [];

    foreach (var allowedHost in allowedHosts.Where(
                 host => host != "*"))
    {
        options.AllowedHosts.Add(allowedHost);
    }
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(180);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        SecurityPolicyNames.Authentication,
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(
        SecurityPolicyNames.DataAccess,
        context => RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey:
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var recorder = context.HttpContext.RequestServices
            .GetRequiredService<ISecurityEventRecorder>();
        var timeProvider = context.HttpContext.RequestServices
            .GetRequiredService<TimeProvider>();

        try
        {
            Guid? userId = Guid.TryParse(
                context.HttpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier),
                out var parsedUserId)
                ? parsedUserId
                : null;

            await recorder.RecordAsync(
                new SecurityEventRecord(
                    SecurityEventType.RateLimitRejected,
                    userId,
                    context.HttpContext.User.FindFirstValue(
                        ClaimTypes.Email)
                        ?? context.HttpContext.User.Identity?.Name,
                    context.HttpContext.Connection
                        .RemoteIpAddress?.ToString(),
                    context.HttpContext.Request.Headers.UserAgent
                        .ToString(),
                    context.HttpContext.Request.Path,
                    timeProvider.GetUtcNow()),
                cancellationToken);
        }
        catch
        {
            // Rejection must not depend on event persistence.
        }
    };
});

var dataProtection = builder.Services
    .AddDataProtection()
    .SetApplicationName("ConstructionProjectManagement");

var keyPath =
    builder.Configuration["Security:DataProtection:KeysPath"];

if (!string.IsNullOrWhiteSpace(keyPath))
{
    if (!Path.IsPathRooted(keyPath))
    {
        keyPath = Path.GetFullPath(
            Path.Combine(
                builder.Environment.ContentRootPath,
                keyPath));
    }

    Directory.CreateDirectory(keyPath);

    dataProtection.PersistKeysToFileSystem(
        new DirectoryInfo(keyPath));
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready")
    });

app.MapAuthenticationEndpoints();

app.MapGet(
        "/reports/export/{report}",
        async (
            string report,
            Guid? projectId,
            DateOnly? from,
            DateOnly? to,
            ExportReportCsvHandler exporter,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<ReportKind>(
                    report,
                    ignoreCase: true,
                    out var reportKind))
            {
                return Results.BadRequest(
                    "Unknown report type.");
            }

            if (from is not null &&
                to is not null &&
                to < from)
            {
                return Results.BadRequest(
                    "The report end date cannot be before the start date.");
            }

            var file = await exporter.HandleAsync(
                reportKind,
                new ReportingFilter(
                    projectId,
                    from,
                    to),
                cancellationToken);

            return Results.File(
                file.Content,
                contentType: "text/csv; charset=utf-8",
                fileDownloadName: file.FileName);
        })
    .RequireAuthorization(Permissions.Reports.Export)
    .RequireRateLimiting(SecurityPolicyNames.DataAccess);

app.MapGet(
        "/files/daily-reports/{reportId:guid}/attachments/{attachmentId:guid}",
        async (
            Guid reportId,
            Guid attachmentId,
            GetDailyReportAttachmentFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var file = await handler.HandleAsync(
                reportId,
                attachmentId,
                cancellationToken);

            return file is null
                ? Results.NotFound()
                : Results.File(
                    file.Content,
                    contentType: file.ContentType,
                    fileDownloadName: file.FileName,
                    enableRangeProcessing: true);
        })
    .RequireAuthorization(Permissions.DailyReports.View)
    .RequireRateLimiting(SecurityPolicyNames.DataAccess);

app.MapGet(
        "/files/documents/{documentId:guid}/revisions/{revisionId:guid}",
        async (
            Guid documentId,
            Guid revisionId,
            GetDocumentRevisionFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var file = await handler.HandleAsync(
                documentId,
                revisionId,
                cancellationToken);

            return file is null
                ? Results.NotFound()
                : Results.File(
                    file.Content,
                    contentType: file.ContentType,
                    fileDownloadName: file.FileName,
                    enableRangeProcessing: true);
        })
    .RequireAuthorization(Permissions.Documents.View)
    .RequireRateLimiting(SecurityPolicyNames.DataAccess);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.EnsureBootstrapAdministratorAsync(
    builder.Configuration);

app.Run();

public partial class Program;
