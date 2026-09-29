using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.IntegrationTests.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcomCourse.IntegrationTests.Orders;

public class OrdersIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Guid _customerId = Guid.NewGuid();
    private Guid _defaultCategoryId;

    public OrdersIntegrationTests()
    {
        var dbName = $"InMemoryTestDb_{Guid.NewGuid()}";

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
                        _ => { }
                    );

                services.RemoveAll<DbContextOptions<EcomCourseDbContext>>();
                services.RemoveAll<EcomCourseDbContext>();

                var inMemoryServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<EcomCourseDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.UseInternalServiceProvider(inMemoryServiceProvider);
                    options.ConfigureWarnings(w =>
                        w.Ignore(InMemoryEventId.TransactionIgnoredWarning)
                    );
                });
            });
        });

        _client = _factory.CreateClient();

        _client.DefaultRequestHeaders.Add("X-Test-CustomerId", _customerId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Customer");
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

        var category = Category.Create("Electronics").Value!;
        _defaultCategoryId = category.Id;
        dbContext.Categories.Add(category);

        var customer = Customer
            .Create(
                _customerId,
                "Test Customer",
                $"{_customerId}@example.com",
                "Khreshchatyk St 1",
                "Kyiv",
                "01001",
                "Ukraine"
            )
            .Value!;
        dbContext.Customers.Add(customer);

        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateOrder_And_GetOrderWithLines_ShouldReturnCorrectData()
    {
        var firstProduct = await SeedProductAsync("Product 1", 100m, Currency.USD, "SKU-0001");
        var secondProduct = await SeedProductAsync("Product 2", 50m, Currency.USD, "SKU-0002");

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
        var line = orderDetails.Lines.First(l => l.Quantity == 2);
        Assert.Equal(100m, line.UnitPrice);
        Assert.Equal(Currency.USD, line.Currency);
        Assert.Equal(200m, line.LineTotal);
    }

    [Fact]
    public async Task CreateOrder_ShouldIgnoreClientSuppliedCustomerId_AndUseAuthenticatedCustomer()
    {
        var spoofedCustomerId = Guid.NewGuid();
        var product = await SeedProductAsync("Spoof Test Product", 100m, Currency.USD, "SKU-SPDF");

        var payload = new
        {
            customerId = spoofedCustomerId,
            items = new[] { new { productId = product.Id, quantity = 1 } },
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
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new { items = Array.Empty<object>() }),
        };
        request.Headers.Add("X-Test-Role", "Admin");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ShouldReturn403_WhenCallerHasNoCustomerRole()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders");
        request.Headers.Add("X-Test-Role", "Admin");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn401_ForAnonymous()
    {
        var orderId = await CreateSampleOrderAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{orderId}");
        request.Headers.Add("X-Test-Anonymous", "true");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn403_WhenCalledByDifferentCustomer()
    {
        var orderId = await CreateSampleOrderAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{orderId}");
        request.Headers.Add("X-Test-CustomerId", Guid.NewGuid().ToString());
        request.Headers.Add("X-Test-Role", "Customer");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn401_WhenAnonymous(string action)
    {
        var orderId = await CreateSampleOrderAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/{action}");
        request.Headers.Add("X-Test-Anonymous", "true");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn403_WhenCalledByDifferentCustomer(string action)
    {
        var orderId = await CreateSampleOrderAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/{action}");
        request.Headers.Add("X-Test-CustomerId", Guid.NewGuid().ToString());
        request.Headers.Add("X-Test-Role", "Customer");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByOwningCustomer(string action)
    {
        var orderId = await CreateSampleOrderAsync();

        var response = await _client.PostAsync($"/api/orders/{orderId}/{action}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByAdmin(string action)
    {
        var orderId = await CreateSampleOrderAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/{action}");
        request.Headers.Add("X-Test-Role", "Admin");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Product> SeedProductAsync(
        string name,
        decimal amount,
        Currency currency,
        string sku
    )
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

        var createResult = Product.Create(name, amount, currency, sku, _defaultCategoryId);

        if (createResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Не вдалося створити продукт: {createResult.Error.Code} - {createResult.Error.Description}"
            );
        }

        var product = createResult.Value!;

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    private async Task<Guid> CreateSampleOrderAsync()
    {
        var product = await SeedProductAsync(
            "Quick Order Product",
            15m,
            Currency.USD,
            $"SKU-{Guid.NewGuid():N}"
        );

        var command = new CreateOrderCommand(
            _customerId,
            new List<OrderLineItemRequest> { new(product.Id, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/orders", command);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
