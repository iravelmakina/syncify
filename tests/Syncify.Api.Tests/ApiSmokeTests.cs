using System.Net;
using System.Net.Http.Json;

namespace Syncify.Api.Tests;

public sealed class ApiSmokeTests(SyncifyWebApplicationFactory factory)
    : IClassFixture<SyncifyWebApplicationFactory>
{
    [Fact]
    public async Task HealthEndpoint_Returns200_WithHealthyStatus()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("healthy", body.Status);
    }

    [Fact]
    public async Task Endpoints_WithoutUserIdHeader_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/connections");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListConnections_WithValidUserIdHeader_Returns200()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-ID", Guid.NewGuid().ToString());

        var response = await client.GetAsync("/connections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record HealthResponse(string Status);
}
