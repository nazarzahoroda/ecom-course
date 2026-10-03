using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace EcomCourse.IntegrationTests.Security;

[Collection("IntegrationTests")]
public class MiddlewareSecurityTests
{
    private readonly HttpClient _client;

    public MiddlewareSecurityTests(
        CustomWebApplicationFactory<Program, EcomCourseDbContext, IdentityDbContext> factory
    )
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cors_ShouldRejectRequest_WhenOriginIsNotAllowed()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
        request.Headers.Add("Origin", "http://unauthorized-malicious-site.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    [Fact]
    public async Task Authenticate_ShouldReturnProblemDetails_WhenTokenIsMalformed()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/Cart");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "invalid.malformed.jwt.token"
        );

        var response = await _client.SendAsync(request);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();

        var jsonDocument = JsonDocument.Parse(content);
        jsonDocument.RootElement.TryGetProperty("status", out _).Should().BeTrue();
    }
}
