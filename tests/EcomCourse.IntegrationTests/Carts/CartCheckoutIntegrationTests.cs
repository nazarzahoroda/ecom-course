using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.IntegrationTests.TestSupport;
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
        _factory = factory;
        _client = factory.WithTestAuthentication().CreateClient();
    }

    [Fact]
    public async Task Checkout_WhenCartIsEmpty_ReturnsConflictDomainError()
    {
        var customerId = Guid.NewGuid();

        _client.AuthenticateAs(customerId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var cartResult = Cart.Create(customerId);
            var cart = cartResult.Value!;

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
            SeedCustomer(db, customerId, "Khreshchatyk St 1", "Kyiv", "01001", "Ukraine");
            await db.SaveChangesAsync();
        }

        _client.AuthenticateAs(customerId);

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
            Assert.Equal(Currency.USD, order.Lines.First().Currency);
            Assert.Equal(Currency.USD, order.Currency);
            Assert.Equal("Khreshchatyk St 1", order.ShippingAddress.Street);
            Assert.Equal("Kyiv", order.ShippingAddress.City);
        }
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenCartHasMixedCurrencyItems()
    {
        var customerId = Guid.NewGuid();

        var categoryResult = Category.Create("Test Category");
        var category = categoryResult.Value!;

        var usdProduct = Product.Create(
            "USD Product",
            100.00m,
            Currency.USD,
            $"PRD-{Random.Shared.Next(1000, 9999)}",
            category.Id
        ).Value!;

        var uahProduct = Product.Create(
            "UAH Product",
            100.00m,
            Currency.UAH,
            $"PRD-{Random.Shared.Next(1000, 9999)}",
            category.Id
        ).Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            db.Categories.Add(category);
            db.Products.AddRange(usdProduct, uahProduct);
            SeedCustomer(db, customerId, "Khreshchatyk St 1", "Kyiv", "01001", "Ukraine");
            await db.SaveChangesAsync();
        }

        _client.AuthenticateAs(customerId);

        await _client.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = usdProduct.Id, Quantity = 1 }
        );
        await _client.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = uahProduct.Id, Quantity = 1 }
        );

        var checkoutRes = await _client.PostAsync("/api/Cart/checkout", null);

        Assert.Equal(HttpStatusCode.BadRequest, checkoutRes.StatusCode);
    }

    [Fact]
    public async Task Checkout_SnapshotsShippingAddress_IndependentOfLaterCustomerAddressChanges()
    {
        var customerId = Guid.NewGuid();

        var categoryResult = Category.Create("Test Category");
        var category = categoryResult.Value!;

        var priceResult = Price.Create(50.00m, Currency.USD);
        var randomSku = $"PRD-{Random.Shared.Next(1000, 9999)}";
        var skuResult = SKU.Create(randomSku);

        var productResult = Product.Create(
            "Sample Product",
            priceResult.Value!.Amount,
            priceResult.Value!.Currency,
            skuResult.Value!.Value,
            category.Id
        );
        var product = productResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            db.Categories.Add(category);
            db.Products.Add(product);
            SeedCustomer(db, customerId, "Original St 1", "Kyiv", "01001", "Ukraine");
            await db.SaveChangesAsync();
        }

        _client.AuthenticateAs(customerId);

        await _client.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product.Id, Quantity = 1 }
        );

        var checkoutRes = await _client.PostAsync("/api/Cart/checkout", null);
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        var orderId = await checkoutRes.Content.ReadFromJsonAsync<Guid>();

        // Customer moves after the order was placed.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Customers SET Address_Street = {"New St 99"}, Address_City = {"Lviv"} WHERE Id = {customerId}");
        }

        var getRes = await _client.GetAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var order = await getRes.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal("Original St 1", order.ShippingAddress.Street);
        Assert.Equal("Kyiv", order.ShippingAddress.City);
    }

    private static void SeedCustomer(
        EcomCourseDbContext db,
        Guid customerId,
        string street,
        string city,
        string postalCode,
        string country)
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            "Test Customer",
            $"{customerId}@example.com",
            street,
            city,
            postalCode,
            country).Value!;

        typeof(Customer).BaseType!
            .GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(customer, customerId);

        db.Customers.Add(customer);
    }
}
