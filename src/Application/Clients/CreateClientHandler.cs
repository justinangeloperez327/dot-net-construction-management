using Domain.Clients;

namespace Application.Clients;

public sealed record CreateClientRequest(
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);

public sealed class CreateClientHandler(
    IClientRepository clients)
{
    public async Task<ClientActionResult> HandleAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = Client.Create(
                request.Name,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.Address);

            await clients.AddAsync(client, cancellationToken);
            await clients.SaveChangesAsync(cancellationToken);

            return ClientActionResult.Success(client.Id);
        }
        catch (ArgumentException exception)
        {
            return ClientActionResult.Failure(exception.Message);
        }
    }
}
