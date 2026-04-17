using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Syncify.Shared.Enums;
using Syncify.Sync.Infrastructure.Http;

namespace Syncify.Sync.Infrastructure.Tests;

public class HttpConnectionServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly HttpClient _httpClient;
    private readonly HttpConnectionService _service;

    public HttpConnectionServiceTests()
    {
        _httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://connections-service") };
        var options = Microsoft.Extensions.Options.Options.Create(new ConnectionsServiceOptions { BaseUrl = "http://connections-service" });
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        _service = new HttpConnectionService(_httpClient, options, httpContextAccessor);
    }

    [Fact]
    public async Task GetCalendarAccessAsync_ValidResponse_ReturnsAccess()
    {
        var calendarId = Guid.NewGuid();
        var json = "{\"Access\": \"ReadWrite\"}";
        
        SetupMockResponse(HttpStatusCode.OK, json);

        var result = await _service.GetCalendarAccessAsync(calendarId);

        Assert.Equal(CalendarAccess.ReadWrite, result);
    }

    [Fact]
    public async Task GetCalendarAccessAsync_NotFound_ThrowsInvalidOperationException()
    {
        SetupMockResponse(HttpStatusCode.NotFound, string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetCalendarAccessAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetProviderCalendarAccessTokenAsync_ValidResponse_ReturnsTokenAndId()
    {
        var calendarId = Guid.NewGuid();
        var json = "{\"AccessToken\": \"token-abc\", \"ProviderCalendarId\": \"primary\"}";
        
        SetupMockResponse(HttpStatusCode.OK, json);

        var result = await _service.GetProviderCalendarAccessTokenAsync(calendarId);

        Assert.Equal("token-abc", result.AccessToken);
        Assert.Equal("primary", result.ProviderCalendarId);
    }

    private void SetupMockResponse(HttpStatusCode code, string content)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = code,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
    }
}
