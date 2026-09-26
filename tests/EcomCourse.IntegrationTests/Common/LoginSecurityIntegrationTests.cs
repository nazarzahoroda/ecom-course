using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EcomCourse.IntegrationTests.Common;

public class LoginSecurityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoginSecurityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithTestAuthentication().CreateClient();
    }

    [Fact]
    public async Task Login_UnknownEmailAndWrongPassword_ShouldReturnByteIdenticalResponses()
    {
        var existingEmail = $"user-{Guid.NewGuid()}@example.com";
        var correctPassword = "StrongPassword123!";

        var registerDto = new
        {
            Email = existingEmail,
            Password = correctPassword,
            Name = "TestUserq",
            Street = "Main St 1",
            City = "Kyiv",
            PostalCode = "01001",
            Country = "Ukraine",
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var wrongPasswordPayload = new LoginDto
        {
            Email = existingEmail,
            Password = "WrongPassword999!",
        };

        var unknownEmailPayload = new LoginDto
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "WrongPassword999!",
        };

        var wrongPasswordContent = new StringContent(
            JsonSerializer.Serialize(wrongPasswordPayload),
            Encoding.UTF8,
            "application/json"
        );

        var unknownEmailContent = new StringContent(
            JsonSerializer.Serialize(unknownEmailPayload),
            Encoding.UTF8,
            "application/json"
        );

        var wrongPasswordResponse = await _client.PostAsync(
            "/api/auth/login",
            wrongPasswordContent
        );
        var unknownEmailResponse = await _client.PostAsync("/api/auth/login", unknownEmailContent);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPasswordResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmailResponse.StatusCode);

        var wrongPasswordBytes = await wrongPasswordResponse.Content.ReadAsByteArrayAsync();
        var unknownEmailBytes = await unknownEmailResponse.Content.ReadAsByteArrayAsync();

        Assert.Equal(wrongPasswordBytes, unknownEmailBytes);
    }
}
