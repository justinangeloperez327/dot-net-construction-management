using Application.Clients;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ClientRepository(
    ApplicationDbContext dbContext)
    : IClientRepository
{
    public Task<Client?> GetByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Clients
            .SingleOrDefaultAsync(
                client => client.Id == clientId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> ListAsync(
        string? search,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(
                dbContext.Clients.AsNoTracking(),
                search,
                isActive)
            .OrderBy(client => client.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Clients
            .AsNoTracking()
            .Where(client => client.IsActive)
            .OrderBy(client => client.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilter(
                dbContext.Clients.AsNoTracking(),
                search,
                isActive)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Client client,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Clients.AddAsync(
            client,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Client> ApplyFilter(
        IQueryable<Client> query,
        string? search,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(client =>
                client.Name.Contains(search) ||
                (client.ContactPerson != null &&
                 client.ContactPerson.Contains(search)) ||
                (client.Email != null &&
                 client.Email.Contains(search)));
        }

        if (isActive is not null)
        {
            query = query.Where(
                client => client.IsActive == isActive.Value);
        }

        return query;
    }
}
