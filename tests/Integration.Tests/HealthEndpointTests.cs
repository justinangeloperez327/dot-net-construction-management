using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Integration.Tests;

public sealed class HealthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_health_endpoint_returns_success()
    {
        using var response = await _client.GetAsync(
            "/health/live",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
