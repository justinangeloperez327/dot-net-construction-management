using Domain.Approvals;
using Domain.Clients;
using Domain.DailyReports;
using Domain.Documents;
using Domain.Projects;
using Domain.Suppliers;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<DailyReport> DailyReports => Set<DailyReport>();

    public DbSet<DailyReportActivity> DailyReportActivities =>
        Set<DailyReportActivity>();

    public DbSet<DailyReportManpowerEntry> DailyReportManpower =>
        Set<DailyReportManpowerEntry>();

    public DbSet<DailyReportEquipmentEntry> DailyReportEquipment =>
        Set<DailyReportEquipmentEntry>();

    public DbSet<DailyReportSiteIssue> DailyReportSiteIssues =>
        Set<DailyReportSiteIssue>();

    public DbSet<DailyReportAttachment> DailyReportAttachments =>
        Set<DailyReportAttachment>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentRevision> DocumentRevisions =>
        Set<DocumentRevision>();

    public DbSet<ApprovalRequest> ApprovalRequests =>
        Set<ApprovalRequest>();

    public DbSet<ApprovalStep> ApprovalSteps =>
        Set<ApprovalStep>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
