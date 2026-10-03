using System.Net;
using System.Net.Http.Json;
using EcomCourse.Domain.Customers;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.IntegrationTests.Infrastructure;
using EcomCourse.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EcomCourse.IntegrationTests.Authentication;

[Collection("IntegrationTests")]
public class RegistrationTransactionIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<
        Program,
        EcomCourseDbContext,
        IdentityDbContext
    > _factory;

    public RegistrationTransactionIntegrationTests(
        CustomWebApplicationFactory<Program, EcomCourseDbContext, IdentityDbContext> factory
    )
    {
        _factory = factory;
        _client = factory.WithTestAuthentication().CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Register_WhenCustomerCreationFails_RollsBackIdentityUser()
    {
        var targetEmail = $"rollback-{Guid.NewGuid()}@example.com";
        var correctPassword = "StrongPassword123!";

        // 1. Створюємо клієнта в EcomCourseDbContext, щоб викликати помилку дубліката
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            // Створіть сутність Customer відповідно до вашого доменного конструктора/фабрики:
            var customerResult = Customer.Create(
                Guid.NewGuid(),
                "Existing Client",
                targetEmail,
                "Main St 1",
                "Kyiv",
                "01001",
                "Ukraine"
            );

            var customer = customerResult.Value;
            db.Customers.Add(customer!);
            await db.SaveChangesAsync();
        }

        var registerDto = new
        {
            Email = targetEmail,
            Password = correctPassword,
            Name = "TestUserq",
            Street = "Main St 1",
            City = "Kyiv",
            PostalCode = "01001",
            Country = "Ukraine",
        };

        // 2. Викликаємо реєстрацію з тим самим email
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Тепер запит має повернути помилку (400 або 409 або 500 залежно від обробки)
        Assert.False(registerResponse.IsSuccessStatusCode);

        // 3. Перевіряємо, що IdentityUser відкотився
        using var verifyScope = _factory.Services.CreateScope();

        // Якщо Identity зберігається в IdentityDbContext:
        var identityDb = verifyScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var identityUserExists = await identityDb.Users.AnyAsync(user => user.Email == targetEmail);

        Assert.False(
            identityUserExists,
            "IdentityUser мав бути відкочений після невдачі збереження Customer."
        );
    }
}
