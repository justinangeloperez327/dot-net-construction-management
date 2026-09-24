namespace Domain.DailyReports;

public sealed class DailyReportAttachment
{
    private DailyReportAttachment()
    {
    }

    internal DailyReportAttachment(
        Guid id,
        Guid dailyReportId,
        string storageKey,
        string fileName,
        string contentType,
        long sizeBytes,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAt,
        string? caption)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Attachment ID is required.",
                nameof(id));
        }

        if (dailyReportId == Guid.Empty)
        {
            throw new ArgumentException(
                "Daily report ID is required.",
                nameof(dailyReportId));
        }

        if (uploadedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Uploaded-by user ID is required.",
                nameof(uploadedByUserId));
        }

        storageKey = storageKey.Trim();
        fileName = fileName.Trim();
        contentType = contentType.Trim();

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException(
                "Storage key is required.",
                nameof(storageKey));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Content type is required.",
                nameof(contentType));
        }

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sizeBytes),
                "File size must be greater than zero.");
        }

        Id = id;
        DailyReportId = dailyReportId;
        StorageKey = storageKey;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
        Caption = NormalizeOptional(caption);
    }

    public Guid Id { get; private set; }

    public Guid DailyReportId { get; private set; }

    public string StorageKey { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public DateTimeOffset UploadedAt { get; private set; }

    public string? Caption { get; private set; }

    public void UpdateCaption(string? caption)
    {
        Caption = NormalizeOptional(caption);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
