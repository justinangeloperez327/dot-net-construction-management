using Application.Common.Persistence;

namespace Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    ApplicationDbContext dbContext)
    : IUnitOfWork
{
    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
