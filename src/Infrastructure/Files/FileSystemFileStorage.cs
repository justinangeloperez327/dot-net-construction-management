using Application.Common.Files;

namespace Infrastructure.Files;

public sealed class FileSystemFileStorage(
    FileStorageOptions options)
    : IFileStorage
{
    private readonly string _rootPath =
        Path.GetFullPath(options.RootPath);

    public async Task WriteAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException(
                "Storage path does not have a directory.");

        Directory.CreateDirectory(directory);

        await using var file = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await content.CopyToAsync(file, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = ResolvePath(storageKey);

        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = ResolvePath(storageKey);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            Path.IsPathRooted(storageKey))
        {
            throw new ArgumentException(
                "Storage key must be a relative path.",
                nameof(storageKey));
        }

        var normalized = storageKey
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, normalized));

        var relative = Path.GetRelativePath(
            _rootPath,
            fullPath);

        if (relative == ".." ||
            relative.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal) ||
            Path.IsPathRooted(relative))
        {
            throw new ArgumentException(
                "Storage key resolves outside the storage root.",
                nameof(storageKey));
        }

        return fullPath;
    }
}
