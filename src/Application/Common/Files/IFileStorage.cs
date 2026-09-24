namespace Application.Common.Files;

public interface IFileStorage
{
    Task WriteAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}

public static class FileUploadPolicy
{
    public const long MaxFileSizeBytes = 25L * 1024 * 1024;

    public const int MaxFileNameLength = 255;

    public const int MaxCaptionLength = 500;

    private static readonly IReadOnlyDictionary<string, string[]> ExtensionsByContentType =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = [".jpg", ".jpeg"],
            ["image/png"] = [".png"],
            ["image/webp"] = [".webp"],
            ["application/pdf"] = [".pdf"]
        };

    public static string? ValidateAndGetExtension(
        string fileName,
        string contentType,
        long sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            return "The selected file is empty.";
        }

        if (sizeBytes > MaxFileSizeBytes)
        {
            return $"Files cannot exceed {MaxFileSizeBytes / 1024 / 1024} MB.";
        }

        var safeName = Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeName))
        {
            return "File name is required.";
        }

        if (safeName.Length > MaxFileNameLength)
        {
            return $"File names cannot exceed {MaxFileNameLength} characters.";
        }

        if (!ExtensionsByContentType.TryGetValue(
                contentType.Trim(),
                out var allowedExtensions))
        {
            return "Only JPEG, PNG, WebP, and PDF files are allowed.";
        }

        var extension = Path.GetExtension(safeName);

        if (!allowedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            return "The file extension does not match the declared file type.";
        }

        return null;
    }

    public static string GetNormalizedExtension(string fileName) =>
        Path.GetExtension(Path.GetFileName(fileName))
            .ToLowerInvariant();
}
