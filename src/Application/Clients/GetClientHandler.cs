namespace Application.Clients;

public sealed class GetClientHandler(
    IClientRepository clients)
{
    public async Task<ClientDetails?> HandleAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await clients.GetByIdAsync(
            clientId,
            cancellationToken);

        return client is null
            ? null
            : new ClientDetails(
                client.Id,
                client.Name,
                client.ContactPerson,
                client.Email,
                client.Phone,
                client.Address,
                client.IsActive);
    }
}
