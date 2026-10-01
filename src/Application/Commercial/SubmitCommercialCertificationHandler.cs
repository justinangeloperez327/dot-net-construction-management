using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.PaymentApplications;
using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.Approvals;
using Domain.PaymentApplications;
using Domain.PurchaseOrders;

namespace Application.Commercial;

public sealed record SubmitCommercialCertificationCommand(
    IReadOnlyList<ApprovalStepInput> Steps);

public sealed class SubmitCommercialCertificationHandler(
    ICommercialCertificationRepository certifications,
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IApprovalRequestRepository approvals,
    IUserDirectory users,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
        SubmitCommercialCertificationCommand command,
        CancellationToken cancellationToken = default)
    {
        var certification = await certifications.GetByIdAsync(
            certificationId,
            cancellationToken);

        if (certification is null)
        {
            return CommercialCertificationActionResult.Failure(
                "Commercial certification was not found.");
        }

        var paymentApplication = await paymentApplications.GetByIdAsync(
            certification.PaymentApplicationId,
            cancellationToken);

        if (paymentApplication is null ||
            paymentApplication.Status != PaymentApplicationStatus.Approved)
        {
            return CommercialCertificationActionResult.Failure(
                "The payment application must remain approved.");
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            paymentApplication.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null ||
            purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            return CommercialCertificationActionResult.Failure(
                "The source purchase order must remain approved.");
        }

        if (command.Steps.Count == 0)
        {
            return CommercialCertificationActionResult.Failure(
                "At least one approval step is required.");
        }

        if (await approvals.HasPendingForSubjectAsync(
                CommercialCertificationApproval.SubjectType,
                certification.Id,
                cancellationToken))
        {
            return CommercialCertificationActionResult.Failure(
                "This commercial certification already has a pending approval.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return CommercialCertificationActionResult.Failure(
                "An authenticated user is required.");
        }

        if (certification.CreatedByUserId != userId)
        {
            return CommercialCertificationActionResult.Failure(
                "Only the certification creator can submit it for approval.");
        }

        var approverIds = command.Steps
            .Select(step => step.ApproverUserId)
            .Distinct()
            .ToArray();

        var approvers = await users.ListByIdsAsync(
            approverIds,
            cancellationToken);

        if (approvers.Count != approverIds.Length ||
            approvers.Any(approver => !approver.IsActive))
        {
            return CommercialCertificationActionResult.Failure(
                "Every approval step must be assigned to an active user.");
        }

        var supplier = await suppliers.GetByIdAsync(
            purchaseOrder.SupplierId,
            cancellationToken);

        try
        {
            var approval = ApprovalRequest.Create(
                certification.ProjectId,
                CommercialCertificationApproval.SubjectType,
                certification.Id,
                certification.CertificateNumber,
                $"Commercial certification: {certification.CertificateNumber}",
                $"Payment application: {paymentApplication.ApplicationNumber}; Supplier: {supplier?.Name ?? "Unknown supplier"}; Claimed: {purchaseOrder.Currency} {certification.ClaimedAmount:N2}; Certified: {purchaseOrder.Currency} {certification.CertifiedAmount:N2}; Payable: {purchaseOrder.Currency} {certification.PayableAmount:N2}",
                userId,
                timeProvider.GetUtcNow(),
                command.Steps
                    .Select(step => new ApprovalStepAssignment(
                        step.Name,
                        step.ApproverUserId))
                    .ToArray());

            certification.SubmitForApproval(approval.Id);

            await approvals.AddAsync(
                approval,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return CommercialCertificationActionResult.Success(
                certification.Id,
                approval.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return CommercialCertificationActionResult.Failure(
                exception.Message);
        }
    }
}
