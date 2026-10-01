using Application.Approvals;
using Domain.Approvals;
using Domain.Commercial;

namespace Application.Commercial;

public sealed class CommercialCertificationApprovalOutcomeHandler(
    ICommercialCertificationRepository certifications)
    : IApprovalSubjectOutcomeHandler
{
    public string SubjectType =>
        CommercialCertificationApproval.SubjectType;

    public async Task ApplyAsync(
        ApprovalRequest approvalRequest,
        CancellationToken cancellationToken = default)
    {
        var certification = await certifications.GetByIdAsync(
            approvalRequest.SubjectId,
            cancellationToken);

        if (certification is null)
        {
            throw new InvalidOperationException(
                "Commercial certification linked to this approval was not found.");
        }

        var outcome = approvalRequest.Status switch
        {
            ApprovalRequestStatus.Approved =>
                CommercialCertificationStatus.Approved,
            ApprovalRequestStatus.Rejected =>
                CommercialCertificationStatus.Rejected,
            ApprovalRequestStatus.Cancelled =>
                CommercialCertificationStatus.Draft,
            ApprovalRequestStatus.Pending =>
                (CommercialCertificationStatus?)null,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (outcome is not null)
        {
            certification.ApplyApprovalOutcome(
                approvalRequest.Id,
                outcome.Value);
        }
    }
}

public sealed class CancelCommercialCertificationHandler(
    ICommercialCertificationRepository certifications)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
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

        try
        {
            certification.Cancel();

            await certifications.SaveChangesAsync(cancellationToken);

            return CommercialCertificationActionResult.Success(
                certification.Id);
        }
        catch (InvalidOperationException exception)
        {
            return CommercialCertificationActionResult.Failure(
                exception.Message);
        }
    }
}
