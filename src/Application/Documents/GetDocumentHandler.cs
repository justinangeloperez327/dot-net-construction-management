using Application.Users;

namespace Application.Documents;

public sealed class GetDocumentHandler(
    IDocumentRepository documents,
    IUserDirectory users)
{
    public async Task<DocumentDetails?> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await documents.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        var userIds = new HashSet<Guid>
        {
            document.CreatedByUserId
        };

        foreach (var revision in document.Revisions)
        {
            userIds.Add(revision.CreatedByUserId);

            if (revision.SubmittedByUserId is Guid submittedBy)
            {
                userIds.Add(submittedBy);
            }

            if (revision.ReviewedByUserId is Guid reviewedBy)
            {
                userIds.Add(reviewedBy);
            }
        }

        var directory = await users.ListByIdsAsync(
            userIds.ToArray(),
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        string UserName(Guid userId) =>
            usersById.GetValueOrDefault(userId)?.Email
            ?? "Unknown user";

        return new DocumentDetails(
            document.Id,
            document.ProjectId,
            document.DocumentNumber,
            document.Title,
            document.Category,
            document.Discipline,
            document.Originator,
            document.Description,
            document.Status,
            UserName(document.CreatedByUserId),
            document.CreatedAt,
            document.Revisions
                .OrderByDescending(revision => revision.CreatedAt)
                .Select(revision => new DocumentRevisionDetails(
                    revision.Id,
                    revision.RevisionCode,
                    revision.ChangeSummary,
                    revision.Status,
                    revision.FileName,
                    revision.ContentType,
                    revision.SizeBytes,
                    UserName(revision.CreatedByUserId),
                    revision.CreatedAt,
                    revision.SubmittedByUserId is Guid submittedBy
                        ? UserName(submittedBy)
                        : null,
                    revision.SubmittedAt,
                    revision.ReviewedByUserId is Guid reviewedBy
                        ? UserName(reviewedBy)
                        : null,
                    revision.ReviewedAt,
                    revision.ReviewComments))
                .ToArray());
    }
}
