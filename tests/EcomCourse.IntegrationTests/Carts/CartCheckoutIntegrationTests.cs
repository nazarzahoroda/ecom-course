using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.IntegrationTests.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcomCourse.IntegrationTests.Carts;

public class CartCheckoutIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CartCheckoutIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme =
                            TestAuthenticationHandler.AuthenticationScheme;

                        options.DefaultChallengeScheme =
                            TestAuthenticationHandler.AuthenticationScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.AuthenticationScheme,
                        options => { });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Checkout_WhenCartIsEmpty_ReturnsConflictDomainError()
    {
        var customerId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Remove("X-Test-CustomerId");
        _client.DefaultRequestHeaders.Add(
            "X-Test-CustomerId",
            customerId.ToString());

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var cart = new Cart(Guid.NewGuid(), customerId);
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync("/api/Cart/checkout", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_FullFlow_Success_UpdatesCartAndSetsCustomerId()
    {
        var customerId = Guid.NewGuid();

        var categoryResult = Category.Create("Test Category");
        var category = categoryResult.Value!;

        var priceResult = Price.Create(100.00m, Currency.USD);
        Assert.True(priceResult.IsSuccess);

        var randomSku = $"PRD-{Random.Shared.Next(1000, 9999)}";

        var skuResult = SKU.Create(randomSku);
        Assert.True(skuResult.IsSuccess);

        var productResult = Product.Create(
            "Sample Product",
            priceResult.Value!.Amount,
            priceResult.Value!.Currency,
            skuResult.Value!.Value,
            category.Id
        );
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            db.Categories.Add(category);
            db.Products.Add(product);
            await db.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Remove("X-Test-CustomerId");
        _client.DefaultRequestHeaders.Add(
            "X-Test-CustomerId",
            customerId.ToString());

        var addRes = await _client.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product.Id, Quantity = 2 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        var checkoutRes = await _client.PostAsync("/api/Cart/checkout", null);
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        var orderId = await checkoutRes.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, orderId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var cart = await db
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            Assert.NotNull(cart);
            Assert.Equal(CartStatus.CheckedOut, cart.Status);

            var order = await db
                .Orders.Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            Assert.NotNull(order);
            Assert.Equal(customerId, order.CustomerId);
            Assert.Single(order.Lines);
            Assert.Equal(product.Id, order.Lines.First().ProductId);
            Assert.Equal(2, order.Lines.First().Quantity);
            Assert.Equal(100.00m, order.Lines.First().UnitPrice);
        }
    }
}
