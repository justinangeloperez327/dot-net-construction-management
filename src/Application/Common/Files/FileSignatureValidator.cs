namespace Application.Common.Files;

public sealed record FileSignatureValidationResult(
    string? Error,
    Stream Content)
{
    public bool Succeeded => Error is null;
}

public static class FileSignatureValidator
{
    private const int SignatureLength = 12;

    public static async Task<FileSignatureValidationResult> ValidateAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            return new FileSignatureValidationResult(
                "The selected file cannot be read.",
                content);
        }

        var startPosition = content.CanSeek
            ? content.Position
            : 0;

        var prefix = new byte[SignatureLength];
        var bytesRead = 0;

        while (bytesRead < prefix.Length)
        {
            var read = await content.ReadAsync(
                prefix.AsMemory(
                    bytesRead,
                    prefix.Length - bytesRead),
                cancellationToken);

            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        var signature = prefix.AsSpan(0, bytesRead);
        var valid = contentType.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" =>
                signature.StartsWith(
                    [0xFF, 0xD8, 0xFF]),
            "image/png" =>
                signature.StartsWith(
                    [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            "image/webp" =>
                signature.Length >= 12 &&
                signature[..4].SequenceEqual(
                    "RIFF"u8) &&
                signature.Slice(8, 4).SequenceEqual(
                    "WEBP"u8),
            "application/pdf" =>
                signature.StartsWith("%PDF-"u8),
            _ => false
        };

        Stream preparedContent;

        if (content.CanSeek)
        {
            content.Position = startPosition;
            preparedContent = content;
        }
        else
        {
            preparedContent = new PrefixReadStream(
                prefix.AsMemory(0, bytesRead).ToArray(),
                content);
        }

        return valid
            ? new FileSignatureValidationResult(
                null,
                preparedContent)
            : new FileSignatureValidationResult(
                "The file contents do not match the declared file type.",
                preparedContent);
    }

    private sealed class PrefixReadStream(
        byte[] prefix,
        Stream remainder)
        : Stream
    {
        private int _prefixOffset;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length =>
            throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            var prefixRead = ReadPrefix(
                buffer.AsSpan(offset, count));

            if (prefixRead == count)
            {
                return prefixRead;
            }

            return prefixRead + remainder.Read(
                buffer,
                offset + prefixRead,
                count - prefixRead);
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var prefixRead = ReadPrefix(buffer.Span);

            if (prefixRead == buffer.Length)
            {
                return prefixRead;
            }

            var remainderRead = await remainder.ReadAsync(
                buffer[prefixRead..],
                cancellationToken);

            return prefixRead + remainderRead;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return ReadAsync(
                    buffer.AsMemory(offset, count),
                    cancellationToken)
                .AsTask();
        }

        private int ReadPrefix(Span<byte> destination)
        {
            var remaining = prefix.Length - _prefixOffset;

            if (remaining <= 0 || destination.Length == 0)
            {
                return 0;
            }

            var count = Math.Min(
                remaining,
                destination.Length);

            prefix.AsSpan(_prefixOffset, count)
                .CopyTo(destination);

            _prefixOffset += count;

            return count;
        }

        public override void Flush()
        {
        }

        public override long Seek(
            long offset,
            SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(
            byte[] buffer,
            int offset,
            int count) =>
            throw new NotSupportedException();
    }
}
