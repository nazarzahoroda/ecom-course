using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.IntegrationTests.Common;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcomCourse.IntegrationTests.Orders;

public class OrdersIntegrationTests
{
    private readonly HttpClient _client;
    private readonly Guid _customerId = Guid.NewGuid();

    public OrdersIntegrationTests()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
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
                    services.RemoveAll<IOrderRepository>();
                    services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
                });
            });

        _client = factory.CreateClient();

        _client.DefaultRequestHeaders.Add(
            "X-Test-CustomerId",
            _customerId.ToString());
    }

    [Fact]
    public async Task CreateOrder_And_GetOrderWithLines_ShouldReturnCorrectData()
    {
        // Arrange
        var customerId = _customerId;
        var command = new CreateOrderCommand(
            customerId,
            new List<OrderLineItemRequest>
            {
                new(Guid.NewGuid(), 2, 100m, Currency.USD),
                new(Guid.NewGuid(), 1, 50m, Currency.USD)
            });

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/orders", command);

        var responseBody = await createResponse.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, orderId);

        // Act
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var orderDetails = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(orderDetails);
        Assert.Equal(orderId, orderDetails.Id);
        Assert.Equal(customerId, orderDetails.CustomerId);

        Assert.Equal(250m, orderDetails.Total);
        Assert.Equal(Currency.USD, orderDetails.Currency);

        // Сheck lines
        Assert.Equal(2, orderDetails.Lines.Count);

        var firstLine = orderDetails.Lines.First(l => l.Quantity == 2);
        Assert.Equal(100m, firstLine.UnitPrice);
        Assert.Equal(Currency.USD, firstLine.Currency);
        Assert.Equal(200m, firstLine.LineTotal);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn401_ForAnonymous_RegardlessOfOrderExistence()
    {
        // Arrange
        var existingOrderId = await CreateOrderAsync();
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var existingResponse = await _client.SendAsync(
            BuildRequest(HttpMethod.Get, $"/api/orders/{existingOrderId}", anonymous: true));
        var missingResponse = await _client.SendAsync(
            BuildRequest(HttpMethod.Get, $"/api/orders/{nonExistentOrderId}", anonymous: true));

        // Assert — anonymous callers can't tell existing and non-existent orders apart.
        Assert.Equal(HttpStatusCode.Unauthorized, existingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missingResponse.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn403_WhenCalledByDifferentCustomer()
    {
        // Arrange
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.SendAsync(
            BuildRequest(HttpMethod.Get, $"/api/orders/{orderId}", asCustomerId: Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn401_WhenAnonymous(string action)
    {
        // Arrange
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.SendAsync(
            BuildRequest(HttpMethod.Post, $"/api/orders/{orderId}/{action}", anonymous: true));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldReturn403_WhenCalledByDifferentCustomer(string action)
    {
        // Arrange
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.SendAsync(
            BuildRequest(HttpMethod.Post, $"/api/orders/{orderId}/{action}", asCustomerId: Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByOwningCustomer(string action)
    {
        // Arrange
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.SendAsync(
            BuildRequest(HttpMethod.Post, $"/api/orders/{orderId}/{action}"));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("pay")]
    [InlineData("cancel")]
    public async Task OrderAction_ShouldSucceed_WhenCalledByAdmin(string action)
    {
        // Arrange
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.SendAsync(
            BuildRequest(HttpMethod.Post, $"/api/orders/{orderId}/{action}", asCustomerId: Guid.NewGuid(), role: "Admin"));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateOrderAsync()
    {
        var command = new CreateOrderCommand(
            _customerId,
            new List<OrderLineItemRequest> { new(Guid.NewGuid(), 1, 10m, Currency.USD) });

        var response = await _client.PostAsJsonAsync("/api/orders", command);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static HttpRequestMessage BuildRequest(
        HttpMethod method,
        string url,
        bool anonymous = false,
        Guid? asCustomerId = null,
        string? role = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (anonymous)
        {
            request.Headers.Add("X-Test-Anonymous", "true");
        }

        if (asCustomerId.HasValue)
        {
            request.Headers.Add("X-Test-CustomerId", asCustomerId.Value.ToString());
        }

        if (role is not null)
        {
            request.Headers.Add("X-Test-Role", role);
        }

        return request;
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = [];

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = _orders.FirstOrDefault(order => order.Id == id);

            return Task.FromResult(order);
        }

        public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders.Add(order);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetByCustomerIdAsync(
            Guid customerId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var filtered = _orders
                .Where(order => order.CustomerId == customerId)
                .ToList();

            var totalCount = filtered.Count;

            var pagedOrders = filtered
                .OrderByDescending(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<(IReadOnlyList<Order>, int)>((pagedOrders, totalCount));
        }
    }
}
