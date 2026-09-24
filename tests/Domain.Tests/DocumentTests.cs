using Domain.Documents;
using Xunit;

namespace Domain.Tests;

public sealed class DocumentTests
{
    [Fact]
    public void Create_normalizes_document_metadata()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            " DRW-001 ",
            " Ground Floor Plan ",
            " Drawing ",
            " Architectural ",
            " Consultant ",
            " Issued document ",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Equal("DRW-001", document.DocumentNumber);
        Assert.Equal("Ground Floor Plan", document.Title);
        Assert.Equal("Drawing", document.Category);
        Assert.Equal("Architectural", document.Discipline);
        Assert.Equal("Consultant", document.Originator);
        Assert.Equal(DocumentStatus.Active, document.Status);
    }

    [Fact]
    public void Archived_document_cannot_be_edited()
    {
        var document = CreateDocument();

        document.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            document.Update(
                "DRW-002",
                "Updated title",
                null,
                null,
                null,
                null));
    }

    [Fact]
    public void Archived_document_can_be_restored()
    {
        var document = CreateDocument();

        document.Archive();
        document.Restore();

        Assert.Equal(DocumentStatus.Active, document.Status);
    }

    private static Document CreateDocument() =>
        Document.Create(
            Guid.NewGuid(),
            "DRW-001",
            "Ground Floor Plan",
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
