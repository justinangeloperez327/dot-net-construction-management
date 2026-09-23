namespace Application.Clients;

public sealed record UpdateClientRequest(
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);

public sealed class UpdateClientHandler(
    IClientRepository clients)
{
    public async Task<ClientActionResult> HandleAsync(
        Guid clientId,
        UpdateClientRequest request,
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

        try
        {
            client.Update(
                request.Name,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.Address);

            await clients.SaveChangesAsync(cancellationToken);

            return ClientActionResult.Success(client.Id);
        }
        catch (ArgumentException exception)
        {
            return ClientActionResult.Failure(exception.Message);
        }
    }
}
