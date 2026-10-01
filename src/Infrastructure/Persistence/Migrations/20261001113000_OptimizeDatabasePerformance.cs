using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDatabasePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(name: "IX_ApprovalRequests_Status_ProjectId", table: "ApprovalRequests", columns: new[] { "Status", "ProjectId" });
            migrationBuilder.CreateIndex(name: "IX_ApprovalSteps_ApproverUserId_Status", table: "ApprovalSteps", columns: new[] { "ApproverUserId", "Status" });
            migrationBuilder.CreateIndex(name: "IX_CommercialCertifications_Status_ProjectId", table: "CommercialCertifications", columns: new[] { "Status", "ProjectId" });
            migrationBuilder.CreateIndex(name: "IX_DailyReports_ProjectId_Status_ReportDate", table: "DailyReports", columns: new[] { "ProjectId", "Status", "ReportDate" });
            migrationBuilder.CreateIndex(name: "IX_DailyReportSiteIssues_Status_DailyReportId", table: "DailyReportSiteIssues", columns: new[] { "Status", "DailyReportId" });
            migrationBuilder.CreateIndex(name: "IX_Deliveries_PurchaseOrderId_Status_DeliveryDate", table: "Deliveries", columns: new[] { "PurchaseOrderId", "Status", "DeliveryDate" });
            migrationBuilder.CreateIndex(name: "IX_Deliveries_Status_ProjectId", table: "Deliveries", columns: new[] { "Status", "ProjectId" });
            migrationBuilder.CreateIndex(name: "IX_Documents_ProjectId_Status_DocumentNumber", table: "Documents", columns: new[] { "ProjectId", "Status", "DocumentNumber" });
            migrationBuilder.CreateIndex(name: "IX_PaymentApplications_ProjectId_ApplicationDate_ApplicationNumber", table: "PaymentApplications", columns: new[] { "ProjectId", "ApplicationDate", "ApplicationNumber" });
            migrationBuilder.CreateIndex(name: "IX_PaymentApplications_Status_ProjectId", table: "PaymentApplications", columns: new[] { "Status", "ProjectId" });
            migrationBuilder.CreateIndex(name: "IX_Projects_Status_TargetCompletionDate_ProjectNumber", table: "Projects", columns: new[] { "Status", "TargetCompletionDate", "ProjectNumber" });
            migrationBuilder.CreateIndex(name: "IX_PurchaseOrders_ProjectId_OrderDate_PurchaseOrderNumber", table: "PurchaseOrders", columns: new[] { "ProjectId", "OrderDate", "PurchaseOrderNumber" });
            migrationBuilder.CreateIndex(name: "IX_PurchaseOrders_Status_ProjectId", table: "PurchaseOrders", columns: new[] { "Status", "ProjectId" });
            migrationBuilder.CreateIndex(name: "IX_PurchaseRequests_Status_ProjectId", table: "PurchaseRequests", columns: new[] { "Status", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_ApprovalRequests_Status_ProjectId", table: "ApprovalRequests");
            migrationBuilder.DropIndex(name: "IX_ApprovalSteps_ApproverUserId_Status", table: "ApprovalSteps");
            migrationBuilder.DropIndex(name: "IX_CommercialCertifications_Status_ProjectId", table: "CommercialCertifications");
            migrationBuilder.DropIndex(name: "IX_DailyReports_ProjectId_Status_ReportDate", table: "DailyReports");
            migrationBuilder.DropIndex(name: "IX_DailyReportSiteIssues_Status_DailyReportId", table: "DailyReportSiteIssues");
            migrationBuilder.DropIndex(name: "IX_Deliveries_PurchaseOrderId_Status_DeliveryDate", table: "Deliveries");
            migrationBuilder.DropIndex(name: "IX_Deliveries_Status_ProjectId", table: "Deliveries");
            migrationBuilder.DropIndex(name: "IX_Documents_ProjectId_Status_DocumentNumber", table: "Documents");
            migrationBuilder.DropIndex(name: "IX_PaymentApplications_ProjectId_ApplicationDate_ApplicationNumber", table: "PaymentApplications");
            migrationBuilder.DropIndex(name: "IX_PaymentApplications_Status_ProjectId", table: "PaymentApplications");
            migrationBuilder.DropIndex(name: "IX_Projects_Status_TargetCompletionDate_ProjectNumber", table: "Projects");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_ProjectId_OrderDate_PurchaseOrderNumber", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_Status_ProjectId", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequests_Status_ProjectId", table: "PurchaseRequests");
        }
    }
}
