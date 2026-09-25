using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Application.Products;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
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
    private readonly FakeProductManager _productManager = new();

    public OrdersIntegrationTests()
    {
        var customer = CreateCustomer(_customerId);

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
                    services.RemoveAll<ICustomerRepository>();
                    services.AddSingleton<ICustomerRepository>(new InMemoryCustomerRepository(customer));
                    services.RemoveAll<IProductManager>();
                    services.AddSingleton<IProductManager>(_productManager);
                });
            });

        _client = factory.CreateClient();

        _client.DefaultRequestHeaders.Add(
            "X-Test-CustomerId",
            _customerId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Customer");
    }

    private static Customer CreateCustomer(Guid customerId)
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            "Test Customer",
            $"{customerId}@example.com",
            "Khreshchatyk St 1",
            "Kyiv",
            "01001",
            "Ukraine").Value!;

        typeof(Customer).BaseType!
            .GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(customer, customerId);

        return customer;
    }

    [Fact]
    public async Task CreateOrder_And_GetOrderWithLines_ShouldReturnCorrectData()
    {
        // Arrange
        var customerId = _customerId;

        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();

        _productManager.SetPrice(firstProductId, 100m, Currency.USD);
        _productManager.SetPrice(secondProductId, 50m, Currency.USD);

        // UnitPrice/Currency aren't part of the wire contract — the server always
        // charges the price and currency from IProductManager, never client input.
        var command = new CreateOrderCommand(
            customerId,
            new List<OrderLineItemRequest>
            {
                new(firstProductId, 2),
                new(secondProductId, 1)
            });

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/orders", command);

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
        Assert.Equal("Khreshchatyk St 1", orderDetails.ShippingAddress.Street);
        Assert.Equal("Kyiv", orderDetails.ShippingAddress.City);

        // Сheck lines
        Assert.Equal(2, orderDetails.Lines.Count);

        var firstLine = orderDetails.Lines.First(l => l.Quantity == 2);
        Assert.Equal(100m, firstLine.UnitPrice);
        Assert.Equal(Currency.USD, firstLine.Currency);
        Assert.Equal(200m, firstLine.LineTotal);
    }

    [Fact]
    public async Task CreateOrder_ShouldIgnoreClientSuppliedCustomerId_AndUseAuthenticatedCustomer()
    {
        // Arrange
        var spoofedCustomerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productManager.SetPrice(productId, 100m, Currency.USD);

        // The request body carries a customerId belonging to a different customer;
        // the API must ignore it and use the authenticated user's id instead.
        var payload = new
        {
            customerId = spoofedCustomerId,
            items = new[] { new { productId, quantity = 1 } }
        };

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/orders", payload);

        // Assert
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
        // Arrange — an authenticated caller without the Customer role (e.g. an
        // Admin-only account with no customer profile of its own).
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new { items = Array.Empty<object>() })
        };
        request.Headers.Remove("X-Test-Role");
        request.Headers.Add("X-Test-Role", "Admin");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ShouldReturn403_WhenCallerHasNoCustomerRole()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders");
        request.Headers.Remove("X-Test-Role");
        request.Headers.Add("X-Test-Role", "Admin");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        var productId = Guid.NewGuid();
        _productManager.SetPrice(productId, 10m, Currency.USD);

        var command = new CreateOrderCommand(
            _customerId,
            new List<OrderLineItemRequest> { new(productId, 1) });

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

    private sealed class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers;

        public InMemoryCustomerRepository(params Customer[] customers)
        {
            _customers = customers.ToList();
        }

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_customers.Any(customer => customer.Email.Equals(email)));
        }

        public Task<bool> AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            _customers.Add(customer);
            return Task.FromResult(true);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_customers.FirstOrDefault(customer => customer.Id == id));
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var removed = _customers.RemoveAll(customer => customer.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }

    private sealed class FakeProductManager : IProductManager
    {
        private readonly Dictionary<Guid, (decimal Amount, Currency Currency)> _products = [];

        public void SetPrice(Guid productId, decimal amount, Currency currency) =>
            _products[productId] = (amount, currency);

        public Task<Result<Guid>> CreateAsync(
            string name,
            decimal amount,
            Currency currency,
            string sku,
            Guid categoryId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_products.TryGetValue(id, out var product))
            {
                return Task.FromResult(Result.Failure<ProductDto>(ProductErrors.NotFound(id)));
            }

            var dto = new ProductDto(id, "Test product", product.Amount, product.Currency, "SKU-TEST", Guid.NewGuid());

            return Task.FromResult(Result.Success(dto));
        }

        public Task<Result<IReadOnlyList<ProductDto>>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            var missingIds = ids.Where(id => !_products.ContainsKey(id)).ToList();

            if (missingIds.Count > 0)
            {
                return Task.FromResult(Result.Failure<IReadOnlyList<ProductDto>>(ProductErrors.NotFound(missingIds[0])));
            }

            IReadOnlyList<ProductDto> dtos = ids
                .Select(id => new ProductDto(id, "Test product", _products[id].Amount, _products[id].Currency, "SKU-TEST", Guid.NewGuid()))
                .ToList();

            return Task.FromResult(Result.Success(dtos));
        }

        public Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result<IReadOnlyList<ProductDto>>> GetTopAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result> UpdateAsync(
            Guid id,
            string name,
            decimal amount,
            Currency currency,
            string sku,
            Guid categoryId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
