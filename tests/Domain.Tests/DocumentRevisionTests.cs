using Domain.Documents;
using Xunit;

namespace Domain.Tests;

public sealed class DocumentRevisionTests
{
    [Fact]
    public void Only_one_open_revision_is_allowed()
    {
        var document = CreateDocument();

        AddRevision(document, "00");

        Assert.Throws<InvalidOperationException>(() =>
            AddRevision(document, "01"));
    }

    [Fact]
    public void Approved_new_revision_supersedes_previous_approved_revision()
    {
        var document = CreateDocument();
        var submitter = Guid.NewGuid();
        var reviewer = Guid.NewGuid();

        var first = AddRevision(document, "00");
        document.SubmitRevision(
            first.Id,
            submitter,
            DateTimeOffset.UtcNow);
        document.ReviewRevision(
            first.Id,
            DocumentRevisionReviewDecision.Approve,
            reviewer,
            DateTimeOffset.UtcNow,
            null);

        var second = AddRevision(document, "01");
        document.SubmitRevision(
            second.Id,
            submitter,
            DateTimeOffset.UtcNow);
        document.ReviewRevision(
            second.Id,
            DocumentRevisionReviewDecision.ApproveWithComments,
            reviewer,
            DateTimeOffset.UtcNow,
            "Accepted with minor comments.");

        Assert.Equal(
            DocumentRevisionStatus.Superseded,
            first.Status);
        Assert.Equal(
            DocumentRevisionStatus.ApprovedWithComments,
            second.Status);
    }

    [Fact]
    public void Reject_and_approve_with_comments_require_comments()
    {
        var document = CreateDocument();
        var revision = AddRevision(document, "00");

        document.SubmitRevision(
            revision.Id,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            document.ReviewRevision(
                revision.Id,
                DocumentRevisionReviewDecision.Reject,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null));
    }

    [Fact]
    public void Document_with_open_revision_cannot_be_archived()
    {
        var document = CreateDocument();

        AddRevision(document, "00");

        Assert.Throws<InvalidOperationException>(
            document.Archive);
    }

    private static DocumentRevision AddRevision(
        Document document,
        string revisionCode)
    {
        var revisionId = Guid.NewGuid();

        return document.AddRevision(
            revisionId,
            revisionCode,
            "Change summary",
            $"documents/{document.Id:N}/revisions/{revisionId:N}.pdf",
            $"{revisionCode}.pdf",
            "application/pdf",
            128,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }

    private static Document CreateDocument() =>
        Document.Create(
            Guid.NewGuid(),
            "DRW-001",
            "Ground Floor Plan",
            "Drawing",
            "Architectural",
            "Consultant",
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
