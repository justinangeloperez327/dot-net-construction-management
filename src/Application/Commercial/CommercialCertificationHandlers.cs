using Application.Common.Authentication;
using Application.PaymentApplications;
using Application.PurchaseOrders;
using Domain.Commercial;
using Domain.PaymentApplications;
using Domain.PurchaseOrders;

namespace Application.Commercial;

public sealed record CreateCommercialCertificationCommand(
    Guid PaymentApplicationId,
    string CertificateNumber,
    DateOnly CertificateDate,
    decimal CertifiedAmount,
    decimal RetentionPercent,
    decimal AdvancePaymentRecoveryAmount,
    string? Notes);

public sealed class CreateCommercialCertificationHandler(
    ICommercialCertificationRepository certifications,
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        CreateCommercialCertificationCommand command,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            command.PaymentApplicationId,
            cancellationToken);

        if (paymentApplication is null)
        {
            return CommercialCertificationActionResult.Failure(
                "Payment application was not found.");
        }

        if (paymentApplication.Status != PaymentApplicationStatus.Approved)
        {
            return CommercialCertificationActionResult.Failure(
                "Only approved payment applications can be commercially certified.");
        }

        if (await certifications.GetByPaymentApplicationIdAsync(
                paymentApplication.Id,
                cancellationToken) is not null)
        {
            return CommercialCertificationActionResult.Failure(
                "A commercial certification already exists for this payment application.");
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

        if (command.CertificateDate < paymentApplication.ApplicationDate)
        {
            return CommercialCertificationActionResult.Failure(
                "Certificate date cannot be before the payment application date.");
        }

        var certificateNumber = command.CertificateNumber.Trim();

        if (await certifications.CertificateNumberExistsAsync(
                paymentApplication.ProjectId,
                certificateNumber,
                cancellationToken: cancellationToken))
        {
            return CommercialCertificationActionResult.Failure(
                "A commercial certificate with this number already exists in the project.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return CommercialCertificationActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            var certification = CommercialCertification.Create(
                paymentApplication.ProjectId,
                paymentApplication.Id,
                certificateNumber,
                command.CertificateDate,
                paymentApplication.ClaimedAmount,
                command.CertifiedAmount,
                command.RetentionPercent,
                command.AdvancePaymentRecoveryAmount,
                command.Notes,
                userId,
                timeProvider.GetUtcNow());

            await certifications.AddAsync(
                certification,
                cancellationToken);

            await certifications.SaveChangesAsync(cancellationToken);

            return CommercialCertificationActionResult.Success(
                certification.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or ArgumentOutOfRangeException
                or InvalidOperationException)
        {
            return CommercialCertificationActionResult.Failure(
                exception.Message);
        }
    }
}

public sealed record UpdateCommercialCertificationCommand(
    string CertificateNumber,
    DateOnly CertificateDate,
    decimal CertifiedAmount,
    decimal RetentionPercent,
    decimal AdvancePaymentRecoveryAmount,
    string? Notes);

public sealed class UpdateCommercialCertificationHandler(
    ICommercialCertificationRepository certifications,
    IPaymentApplicationRepository paymentApplications)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
        UpdateCommercialCertificationCommand command,
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

        if (command.CertificateDate < paymentApplication.ApplicationDate)
        {
            return CommercialCertificationActionResult.Failure(
                "Certificate date cannot be before the payment application date.");
        }

        var certificateNumber = command.CertificateNumber.Trim();

        if (await certifications.CertificateNumberExistsAsync(
                certification.ProjectId,
                certificateNumber,
                certification.Id,
                cancellationToken))
        {
            return CommercialCertificationActionResult.Failure(
                "A commercial certificate with this number already exists in the project.");
        }

        try
        {
            certification.UpdateHeader(
                certificateNumber,
                command.CertificateDate,
                command.CertifiedAmount,
                command.RetentionPercent,
                command.AdvancePaymentRecoveryAmount,
                command.Notes);

            await certifications.SaveChangesAsync(cancellationToken);

            return CommercialCertificationActionResult.Success(
                certification.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or ArgumentOutOfRangeException
                or InvalidOperationException)
        {
            return CommercialCertificationActionResult.Failure(
                exception.Message);
        }
    }
}
