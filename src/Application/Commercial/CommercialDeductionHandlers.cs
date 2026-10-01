namespace Application.Commercial;

public sealed record CommercialDeductionCommand(
    string Description,
    decimal Amount);

public sealed class AddCommercialDeductionHandler(
    ICommercialCertificationRepository certifications)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
        CommercialDeductionCommand command,
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
            certification.AddOtherDeduction(
                command.Description,
                command.Amount);

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

public sealed class UpdateCommercialDeductionHandler(
    ICommercialCertificationRepository certifications)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
        Guid deductionId,
        CommercialDeductionCommand command,
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
            certification.UpdateOtherDeduction(
                deductionId,
                command.Description,
                command.Amount);

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

public sealed class RemoveCommercialDeductionHandler(
    ICommercialCertificationRepository certifications)
{
    public async Task<CommercialCertificationActionResult> HandleAsync(
        Guid certificationId,
        Guid deductionId,
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
            certification.RemoveOtherDeduction(deductionId);

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
