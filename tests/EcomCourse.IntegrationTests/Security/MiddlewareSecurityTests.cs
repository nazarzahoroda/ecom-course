using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EcomCourse.IntegrationTests.Security;

public class MiddlewareSecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MiddlewareSecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cors_ShouldRejectRequest_WhenOriginIsNotAllowed()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
        request.Headers.Add("Origin", "http://unauthorized-malicious-site.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    [Fact]
    public async Task Authenticate_ShouldReturnProblemDetails_WhenTokenIsMalformed()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/protected-endpoint-or-cart");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "invalid.malformed.jwt.token"
        );

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();

        // Перевіряємо, що відповідь має формат ProblemDetails / JSON
        var jsonDocument = JsonDocument.Parse(content);
        jsonDocument.RootElement.TryGetProperty("status", out _).Should().BeTrue();
    }
}
