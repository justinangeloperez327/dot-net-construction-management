using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Integration.Tests;

public sealed class HomePageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HomePageTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Home_page_returns_success()
    {
        using var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
