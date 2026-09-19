using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcomCourse.IntegrationTests.Orders;

public class OrdersIntegrationTests
{
    private const string _testAuthenticationScheme = "TestScheme";
    private readonly HttpClient _client;
    private readonly Guid _customerId = Guid.NewGuid();

    public OrdersIntegrationTests()
    {
        var customer = CreateCustomer(_customerId);

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(new TestCustomer(_customerId));

                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = _testAuthenticationScheme;
                            options.DefaultChallengeScheme = _testAuthenticationScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                            _testAuthenticationScheme,
                            options => { });
                    services.RemoveAll<IOrderRepository>();
                    services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
                    services.RemoveAll<ICustomerStore>();
                    services.AddSingleton<ICustomerStore>(new InMemoryCustomerStore(customer));
                });
            });

        _client = factory.CreateClient();
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
        var command = new CreateOrderCommand(
            customerId,
            new List<OrderLineItemRequest>
            {
                new(Guid.NewGuid(), 2, 100m),
                new(Guid.NewGuid(), 1, 50m)
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

        // Сheck lines
        Assert.Equal(2, orderDetails.Lines.Count);

        var firstLine = orderDetails.Lines.First(l => l.Quantity == 2);
        Assert.Equal(100m, firstLine.UnitPrice);
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
            new List<OrderLineItemRequest> { new(Guid.NewGuid(), 1, 10m) });

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

    private sealed record TestCustomer(Guid CustomerId);

    private sealed class TestAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly TestCustomer _testCustomer;

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            TestCustomer testCustomer)
            : base(options, logger, encoder)
        {
            _testCustomer = testCustomer;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.TryGetValue("X-Test-Anonymous", out var anonymous) &&
                anonymous == "true")
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var customerId = _testCustomer.CustomerId;

            if (Request.Headers.TryGetValue("X-Test-CustomerId", out var customerIdHeader) &&
                Guid.TryParse(customerIdHeader, out var overrideCustomerId))
            {
                customerId = overrideCustomerId;
            }

            var claims = new List<Claim>
            {
                new("CustomerId", customerId.ToString())
            };

            if (Request.Headers.TryGetValue("X-Test-Role", out var role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                _testAuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(
                principal,
                _testAuthenticationScheme);

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
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

    private sealed class InMemoryCustomerStore : ICustomerStore
    {
        private readonly List<Customer> _customers;

        public InMemoryCustomerStore(params Customer[] customers)
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
}
