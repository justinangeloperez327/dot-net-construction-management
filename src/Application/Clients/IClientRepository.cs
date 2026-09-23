using Domain.Clients;

namespace Application.Clients;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Client>> ListAsync(
        string? search,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Client>> ListActiveAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Client client,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
