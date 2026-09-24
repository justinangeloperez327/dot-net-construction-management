using Application;
using Application.Common.Authentication;
using Application.Common.Authorization;
using Application.DailyReports;
using Application.Documents;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Web.Authentication;
using Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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
        Predicate = registration => registration.Tags.Contains("ready")
    });

app.MapAuthenticationEndpoints();

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
    .RequireAuthorization(Permissions.DailyReports.View);

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
    .RequireAuthorization(Permissions.Documents.View);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.EnsureBootstrapAdministratorAsync(
    builder.Configuration);

app.Run();

public partial class Program;
