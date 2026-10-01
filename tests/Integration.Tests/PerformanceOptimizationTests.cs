using Domain.Approvals;
using Domain.Commercial;
using Domain.DailyReports;
using Domain.Deliveries;
using Domain.Documents;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration.Tests;

public sealed class PerformanceOptimizationTests
{
    [Fact]
    public void Hot_path_indexes_are_present_in_the_model()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=SchemaInspection;User ID=sa;Password=NotUsedForConnection1!;TrustServerCertificate=True")
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        AssertIndex<Project>(dbContext,
            nameof(Project.Status),
            nameof(Project.TargetCompletionDate),
            nameof(Project.ProjectNumber));

        AssertIndex<DailyReport>(dbContext,
            nameof(DailyReport.ProjectId),
            nameof(DailyReport.Status),
            nameof(DailyReport.ReportDate));

        AssertIndex<DailyReportSiteIssue>(dbContext,
            nameof(DailyReportSiteIssue.Status),
            nameof(DailyReportSiteIssue.DailyReportId));

        AssertIndex<Document>(dbContext,
            nameof(Document.ProjectId),
            nameof(Document.Status),
            nameof(Document.DocumentNumber));

        AssertIndex<ApprovalRequest>(dbContext,
            nameof(ApprovalRequest.Status),
            nameof(ApprovalRequest.ProjectId));

        AssertIndex<ApprovalStep>(dbContext,
            nameof(ApprovalStep.ApproverUserId),
            nameof(ApprovalStep.Status));

        AssertIndex<PurchaseRequest>(dbContext,
            nameof(PurchaseRequest.Status),
            nameof(PurchaseRequest.ProjectId));

        AssertIndex<PurchaseOrder>(dbContext,
            nameof(PurchaseOrder.Status),
            nameof(PurchaseOrder.ProjectId));

        AssertIndex<Delivery>(dbContext,
            nameof(Delivery.PurchaseOrderId),
            nameof(Delivery.Status),
            nameof(Delivery.DeliveryDate));

        AssertIndex<PaymentApplication>(dbContext,
            nameof(PaymentApplication.ProjectId),
            nameof(PaymentApplication.ApplicationDate),
            nameof(PaymentApplication.ApplicationNumber));

        AssertIndex<CommercialCertification>(dbContext,
            nameof(CommercialCertification.Status),
            nameof(CommercialCertification.ProjectId));
    }

    private static void AssertIndex<TEntity>(
        ApplicationDbContext dbContext,
        params string[] propertyNames)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

        Assert.NotNull(entityType);

        var hasIndex = entityType!
            .GetIndexes()
            .Any(index =>
                index.Properties
                    .Select(property => property.Name)
                    .SequenceEqual(propertyNames));

        Assert.True(
            hasIndex,
            $"Expected index on {typeof(TEntity).Name}({string.Join(", ", propertyNames)}).");
    }
}
