using Application.Common.Files;
using Xunit;

namespace Application.Tests;

public sealed class FileSignatureValidatorTests
{
    [Theory]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    [InlineData("image/webp", new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50 })]
    [InlineData("application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D })]
    public async Task Accepted_type_requires_matching_signature(
        string contentType,
        byte[] bytes)
    {
        await using var content = new MemoryStream(bytes);

        var result = await FileSignatureValidator.ValidateAsync(
            content,
            contentType,
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Equal(0, content.Position);
    }

    [Fact]
    public async Task Mismatched_signature_is_rejected()
    {
        await using var content = new MemoryStream(
            [0x25, 0x50, 0x44, 0x46, 0x2D]);

        var result = await FileSignatureValidator.ValidateAsync(
            content,
            "image/jpeg",
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
    }
}
