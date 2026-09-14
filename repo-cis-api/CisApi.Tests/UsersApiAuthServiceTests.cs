using System.Net;
using System.Text;
using System.Text.Json;
using CisApi.Business.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace CisApi.Tests;

public class UsersApiAuthServiceTests
{
    private readonly Mock<ILogger<UsersApiAuthService>> _loggerMock = new();

    private UsersApiAuthService CreateService(HttpClient httpClient) =>
        new(httpClient, _loggerMock.Object);

    private HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("http://test.local") };
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ReturnsAuthenticatedUser()
    {
        var responseJson = JsonSerializer.Serialize(new { userId = "user-1", login = "john" });
        using var client = CreateHttpClient(HttpStatusCode.OK, responseJson);

        var result = await CreateService(client).ValidateTokenAsync("valid-token");

        Assert.NotNull(result);
        Assert.Equal("user-1", result.UserId);
        Assert.Equal("john", result.Login);
    }

    [Fact]
    public async Task ValidateTokenAsync_NonSuccessStatus_ReturnsNull()
    {
        using var client = CreateHttpClient(HttpStatusCode.Unauthorized, string.Empty);

        var result = await CreateService(client).ValidateTokenAsync("invalid-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_EmptyUserId_ReturnsNull()
    {
        var responseJson = JsonSerializer.Serialize(new { userId = string.Empty, login = "john" });
        using var client = CreateHttpClient(HttpStatusCode.OK, responseJson);

        var result = await CreateService(client).ValidateTokenAsync("valid-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_NullJsonResult_ReturnsNull()
    {
        using var client = CreateHttpClient(HttpStatusCode.OK, "null");

        var result = await CreateService(client).ValidateTokenAsync("valid-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_HttpRequestException_ReturnsNull()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://test.local") };

        var result = await CreateService(client).ValidateTokenAsync("some-token");

        Assert.Null(result);
    }
}
