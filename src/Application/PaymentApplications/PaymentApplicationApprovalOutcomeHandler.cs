using Application.Approvals;
using Domain.Approvals;
using Domain.PaymentApplications;

namespace Application.PaymentApplications;

public sealed class PaymentApplicationApprovalOutcomeHandler(
    IPaymentApplicationRepository paymentApplications)
    : IApprovalSubjectOutcomeHandler
{
    public string SubjectType =>
        PaymentApplicationApproval.SubjectType;

    public async Task ApplyAsync(
        ApprovalRequest approvalRequest,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            approvalRequest.SubjectId,
            cancellationToken);

        if (paymentApplication is null)
        {
            throw new InvalidOperationException(
                "Payment application linked to this approval was not found.");
        }

        var outcome = approvalRequest.Status switch
        {
            ApprovalRequestStatus.Approved =>
                PaymentApplicationStatus.Approved,
            ApprovalRequestStatus.Rejected =>
                PaymentApplicationStatus.Rejected,
            ApprovalRequestStatus.Cancelled =>
                PaymentApplicationStatus.Draft,
            ApprovalRequestStatus.Pending =>
                (PaymentApplicationStatus?)null,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (outcome is not null)
        {
            paymentApplication.ApplyApprovalOutcome(
                approvalRequest.Id,
                outcome.Value);
        }
    }
}

public sealed class CancelPaymentApplicationHandler(
    IPaymentApplicationRepository paymentApplications)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            paymentApplicationId,
            cancellationToken);

        if (paymentApplication is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment application was not found.");
        }

        try
        {
            paymentApplication.Cancel();

            await paymentApplications.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id);
        }
        catch (InvalidOperationException exception)
        {
            return PaymentApplicationActionResult.Failure(
                exception.Message);
        }
    }
}
