using Domain.Approvals;
using Domain.Clients;
using Domain.Commercial;
using Domain.DailyReports;
using Domain.Deliveries;
using Domain.Documents;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;
using Domain.Suppliers;
using Infrastructure.Identity;
using Infrastructure.Persistence.Auditing;
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

    public DbSet<PurchaseRequest> PurchaseRequests =>
        Set<PurchaseRequest>();

    public DbSet<PurchaseRequestItem> PurchaseRequestItems =>
        Set<PurchaseRequestItem>();

    public DbSet<PurchaseOrder> PurchaseOrders =>
        Set<PurchaseOrder>();

    public DbSet<PurchaseOrderItem> PurchaseOrderItems =>
        Set<PurchaseOrderItem>();

    public DbSet<Delivery> Deliveries =>
        Set<Delivery>();

    public DbSet<DeliveryItem> DeliveryItems =>
        Set<DeliveryItem>();

    public DbSet<PaymentApplication> PaymentApplications =>
        Set<PaymentApplication>();

    public DbSet<PaymentApplicationItem> PaymentApplicationItems =>
        Set<PaymentApplicationItem>();

    public DbSet<CommercialCertification> CommercialCertifications =>
        Set<CommercialCertification>();

    public DbSet<CommercialDeduction> CommercialDeductions =>
        Set<CommercialDeduction>();

    public DbSet<AuditLog> AuditLogs =>
        Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
