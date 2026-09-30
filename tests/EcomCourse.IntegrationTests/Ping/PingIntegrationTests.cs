using System.Net;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.IntegrationTests.Infrastructure;

namespace EcomCourse.IntegrationTests.Ping;

[Collection("IntegrationTests")]
public sealed class PingIntegrationTests
{
    private readonly HttpClient _client;

    public PingIntegrationTests(
        CustomWebApplicationFactory<Program, EcomCourseDbContext, IdentityDbContext> factory
    )
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPing_ShouldReturnPong()
    {
        var response = await _client.GetAsync("/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("\"pong\"", body);
    }
}
