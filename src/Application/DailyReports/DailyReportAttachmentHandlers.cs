using Application.Common.Authentication;
using Application.Common.Files;
using Domain.DailyReports;

namespace Application.DailyReports;

public sealed record UploadDailyReportAttachmentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Caption,
    Stream Content);

public sealed record DailyReportAttachmentFile(
    Stream Content,
    string FileName,
    string ContentType);

public sealed class UploadDailyReportAttachmentHandler(
    IDailyReportRepository reports,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        UploadDailyReportAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        if (report is null)
        {
            return DailyReportActionResult.Failure(
                "Daily report was not found.");
        }

        if (report.Status is not DailyReportStatus.Draft and
            not DailyReportStatus.Rejected)
        {
            return DailyReportActionResult.Failure(
                "Attachments can only be changed on draft or rejected reports.");
        }

        var validationError = FileUploadPolicy.ValidateAndGetExtension(
            request.FileName,
            request.ContentType,
            request.SizeBytes);

        if (validationError is not null)
        {
            return DailyReportActionResult.Failure(validationError);
        }

        if (!string.IsNullOrWhiteSpace(request.Caption) &&
            request.Caption.Trim().Length > FileUploadPolicy.MaxCaptionLength)
        {
            return DailyReportActionResult.Failure(
                $"Captions cannot exceed {FileUploadPolicy.MaxCaptionLength} characters.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DailyReportActionResult.Failure(
                "An authenticated user is required.");
        }

        var safeFileName = Path.GetFileName(request.FileName.Trim());
        var attachmentId = Guid.NewGuid();
        var extension = FileUploadPolicy.GetNormalizedExtension(safeFileName);
        var storageKey =
            $"daily-reports/{report.Id:N}/{attachmentId:N}{extension}";

        try
        {
            await fileStorage.WriteAsync(
                storageKey,
                request.Content,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is IOException or UnauthorizedAccessException)
        {
            return DailyReportActionResult.Failure(
                "The attachment could not be stored.");
        }

        try
        {
            report.AddAttachment(
                attachmentId,
                storageKey,
                safeFileName,
                request.ContentType.Trim().ToLowerInvariant(),
                request.SizeBytes,
                userId,
                timeProvider.GetUtcNow(),
                request.Caption);

            await reports.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await fileStorage.DeleteIfExistsAsync(
                storageKey,
                CancellationToken.None);

            throw;
        }

        return DailyReportActionResult.Success(report.Id);
    }
}

public sealed class DeleteDailyReportAttachmentHandler(
    IDailyReportRepository reports,
    IFileStorage fileStorage)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        if (report is null)
        {
            return DailyReportActionResult.Failure(
                "Daily report was not found.");
        }

        var attachment = report.Attachments.SingleOrDefault(
            item => item.Id == attachmentId);

        if (attachment is null)
        {
            return DailyReportActionResult.Failure(
                "Attachment was not found.");
        }

        try
        {
            report.RemoveAttachment(attachmentId);
        }
        catch (InvalidOperationException exception)
        {
            return DailyReportActionResult.Failure(exception.Message);
        }

        await reports.SaveChangesAsync(cancellationToken);

        await fileStorage.DeleteIfExistsAsync(
            attachment.StorageKey,
            cancellationToken);

        return DailyReportActionResult.Success(report.Id);
    }
}

public sealed class GetDailyReportAttachmentFileHandler(
    IDailyReportRepository reports,
    IFileStorage fileStorage)
{
    public async Task<DailyReportAttachmentFile?> HandleAsync(
        Guid reportId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        var attachment = report?.Attachments.SingleOrDefault(
            item => item.Id == attachmentId);

        if (attachment is null)
        {
            return null;
        }

        var content = await fileStorage.OpenReadAsync(
            attachment.StorageKey,
            cancellationToken);

        return content is null
            ? null
            : new DailyReportAttachmentFile(
                content,
                attachment.FileName,
                attachment.ContentType);
    }
}
