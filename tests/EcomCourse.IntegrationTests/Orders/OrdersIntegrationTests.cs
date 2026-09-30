using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.IntegrationTests.Infrastructure;
using EcomCourse.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.IntegrationTests.Orders;

[Collection("IntegrationTests")]
public class OrdersIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<
        Program,
        EcomCourseDbContext,
        IdentityDbContext
    > _factory;
    private readonly Guid _customerId = Guid.NewGuid();

    public OrdersIntegrationTests(
        CustomWebApplicationFactory<Program, EcomCourseDbContext, IdentityDbContext> factory
    )
    {
        _factory = factory;
        _client = factory.WithTestAuthentication().CreateClient();
        _client.AuthenticateAs(_customerId, role: "Customer");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task CreateOrder_And_GetOrderWithLines_ShouldReturnCorrectData()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            SeedCustomer(db, _customerId, "Khreshchatyk St 1", "Kyiv", "01001", "Ukraine");

            var category = Category.Create($"Category-{Guid.NewGuid():N}").Value!;
            db.Categories.Add(category);

            var firstProduct = CreateProduct("Product 1", 100m, Currency.USD, category.Id);
            var secondProduct = CreateProduct("Product 2", 50m, Currency.USD, category.Id);

            db.Products.AddRange(firstProduct, secondProduct);
            await db.SaveChangesAsync();

            var command = new CreateOrderCommand(
                _customerId,
                new List<OrderLineItemRequest> { new(firstProduct.Id, 2), new(secondProduct.Id, 1) }
            );

            var createResponse = await _client.PostAsJsonAsync("/api/orders", command);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, orderId);

            var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var orderDetails = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            Assert.NotNull(orderDetails);
            Assert.Equal(orderId, orderDetails.Id);
            Assert.Equal(_customerId, orderDetails.CustomerId);
            Assert.Equal(250m, orderDetails.Total);
            Assert.Equal(Currency.USD, orderDetails.Currency);
            Assert.Equal("Khreshchatyk St 1", orderDetails.ShippingAddress.Street);
            Assert.Equal("Kyiv", orderDetails.ShippingAddress.City);

            Assert.Equal(2, orderDetails.Lines.Count);

            var firstLine = orderDetails.Lines.First(l => l.Quantity == 2);
            Assert.Equal(100m, firstLine.UnitPrice);
            Assert.Equal(Currency.USD, firstLine.Currency);
            Assert.Equal(200m, firstLine.LineTotal);
        }
    }

    [Fact]
    public async Task CreateOrder_ShouldIgnoreClientSuppliedCustomerId_AndUseAuthenticatedCustomer()
    {
        var spoofedCustomerId = Guid.NewGuid();

        Guid productId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            SeedCustomer(db, _customerId, "Khreshchatyk St 1", "Kyiv", "01001", "Ukraine");

            var category = Category.Create($"Category-{Guid.NewGuid():N}").Value!;
            db.Categories.Add(category);

            var product = CreateProduct("Product 1", 100m, Currency.USD, category.Id);
            db.Products.Add(product);
            await db.SaveChangesAsync();

            productId = product.Id;
        }

        var payload = new
        {
            customerId = spoofedCustomerId,
            items = new[] { new { productId, quantity = 1 } },
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", payload);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        var orderDetails = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(orderDetails);
        Assert.Equal(_customerId, orderDetails.CustomerId);
        Assert.NotEqual(spoofedCustomerId, orderDetails.CustomerId);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn403_WhenCallerHasNoCustomerRole()
    {
        _client.AuthenticateAs(_customerId, role: "Admin");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new { items = Array.Empty<object>() }),
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ShouldReturn403_WhenCallerHasNoCustomerRole()
    {
        _client.AuthenticateAs(_customerId, role: "Admin");

        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn401_ForAnonymous_RegardlessOfOrderExistence()
    {
        var existingOrderId = await CreateOrderInDatabaseAsync();
        var nonExistentOrderId = Guid.NewGuid();

        _client.AuthenticateAsAnonymous();

        var existingResponse = await _client.GetAsync($"/api/orders/{existingOrderId}");
        var missingResponse = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

        Assert.Equal(HttpStatusCode.Unauthorized, existingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missingResponse.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn403_WhenCalledByDifferentCustomer()
    {
        var orderId = await CreateOrderInDatabaseAsync();

        _client.AuthenticateAs(Guid.NewGuid(), role: "Customer");

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn401_WhenAnonymous(string action)
    {
        var orderId = await CreateOrderInDatabaseAsync();

        _client.AuthenticateAsAnonymous();

        var response = await _client.PostAsync($"/api/orders/{orderId}/{action}", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn403_WhenCalledByDifferentCustomer(string action)
    {
        var orderId = await CreateOrderInDatabaseAsync();

        _client.AuthenticateAs(Guid.NewGuid(), role: "Customer");

        var response = await _client.PostAsync($"/api/orders/{orderId}/{action}", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByOwningCustomer(string action)
    {
        var orderId = await CreateOrderInDatabaseAsync();

        _client.AuthenticateAs(_customerId, role: "Customer");

        var response = await _client.PostAsync($"/api/orders/{orderId}/{action}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByAdmin(string action)
    {
        var orderId = await CreateOrderInDatabaseAsync();

        _client.AuthenticateAs(Guid.NewGuid(), role: "Admin");

        var response = await _client.PostAsync($"/api/orders/{orderId}/{action}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateOrderInDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

        SeedCustomer(db, _customerId, "Khreshchatyk St 1", "Kyiv", "01001", "Ukraine");

        var category = Category.Create($"Category-{Guid.NewGuid():N}").Value!;
        db.Categories.Add(category);

        var product = CreateProduct("Product 1", 10m, Currency.USD, category.Id);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var items = new List<(Guid ProductId, int Quantity, decimal UnitPrice, Currency Currency)>
        {
            (product.Id, 1, 10m, Currency.USD),
        };

        var address = Address.Create("Khreshchatyk St 1", "Kyiv", "01001", "Ukraine").Value!;
        var order = Order.Create(_customerId, address, items, DateTime.UtcNow).Value!;

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
    }

    private static Product CreateProduct(
        string name,
        decimal amount,
        Currency currency,
        Guid categoryId
    )
    {
        var price = Price.Create(amount, currency).Value!;
        var sku = SKU.Create($"SKU-{Random.Shared.Next(1000, 9999)}").Value!;
        return Product.Create(name, price.Amount, price.Currency, sku.Value, categoryId).Value!;
    }

    private static void SeedCustomer(
        EcomCourseDbContext db,
        Guid customerId,
        string street,
        string city,
        string postalCode,
        string country
    )
    {
        var customer = Customer
            .Create(
                Guid.NewGuid(),
                "Test Customer",
                $"{customerId}@example.com",
                street,
                city,
                postalCode,
                country
            )
            .Value!;

        typeof(Customer)
            .BaseType!.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(customer, customerId);

        db.Customers.Add(customer);
    }
}
