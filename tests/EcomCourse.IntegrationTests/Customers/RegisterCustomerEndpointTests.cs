using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Customers;
using EcomCourse.Domain.Customers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcomCourse.IntegrationTests.Customers;

public sealed class RegisterCustomerEndpointTests
{
    [Fact]
    public async Task RegisterCustomerReturnsCreatedAndConflictForDuplicateEmail()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "TestConnectionString");

        await using var application = new CustomerApiApplication();
        try
        {
            var client = application
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<ICustomerRepository>();
                        services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
                    });
                })
                .CreateClient();

            var request = new RegisterCustomerRequest(
                Guid.NewGuid(),
                "Ivan",
                "ivan@example.com",
                "Polubotka",
                "Lviv",
                "79066",
                "Ukraine");

            var firstResponse = await client.PostAsJsonAsync("/customers/register", request);
            var secondResponse = await client.PostAsJsonAsync("/customers/register", request);

            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        }
    }

    private sealed record RegisterCustomerRequest(
        Guid UserId,
        string Name,
        string Email,
        string Street,
        string City,
        string PostalCode,
        string Country);

    private sealed class CustomerApiApplication : WebApplicationFactory<Program>
    {
    }

    private sealed class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers = [];

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        {
            var exists = _customers.Any(customer => customer.Email.Equals(email));

            return Task.FromResult(exists);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            _customers.Add(customer);

            return Task.CompletedTask;
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = _customers.FirstOrDefault(customer => customer.Id == id);

            return Task.FromResult(customer);
        }
    }
}
