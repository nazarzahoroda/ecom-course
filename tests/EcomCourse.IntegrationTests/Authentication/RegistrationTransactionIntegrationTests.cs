using System.Net;
using System.Net.Http.Json;
using EcomCourse.Domain.Customers;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EcomCourse.IntegrationTests.Authentication;

public class RegistrationTransactionIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RegistrationTransactionIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WhenCustomerCreationFails_RollsBackIdentityUser()
    {
        var email = $"rollback-{Guid.NewGuid()}@example.com";

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICustomerStore>();
                services.AddScoped<ICustomerStore, FailingCustomerStore>();
            });
        });

        using var client = factory.CreateClient();

        var registerDto = new
        {
            Email = email,
            Password = "StrongPassword123!",
            Name = "Rollback Test",
            Street = "Main St 1",
            City = "Kyiv",
            PostalCode = "01001",
            Country = "Ukraine",
        };

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto
        );

        Assert.False(response.IsSuccessStatusCode);

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<EcomCourseDbContext>();

        var identityUserExists = await db
            .Set<ApplicationUser>()
            .AnyAsync(user => user.Email == email);

        var customerExists = await db.Customers
            .AnyAsync(customer => customer.Email.Value == email);

        Assert.False(identityUserExists);
        Assert.False(customerExists);
    }

    private sealed class FailingCustomerStore : ICustomerStore
    {
        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> AddAsync(
            Customer customer,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Customer?>(null);
        }
    }
}
