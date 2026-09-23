namespace Application.Clients;

public sealed class SetClientActiveHandler(
    IClientRepository clients)
{
    public async Task<ClientActionResult> HandleAsync(
        Guid clientId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var client = await clients.GetByIdAsync(
            clientId,
            cancellationToken);

        if (client is null)
        {
            return ClientActionResult.Failure(
                "Client was not found.");
        }

        client.SetActive(isActive);

        await clients.SaveChangesAsync(cancellationToken);

        return ClientActionResult.Success(client.Id);
    }
}
