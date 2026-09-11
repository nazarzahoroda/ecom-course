using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
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
                });
            });

        _client = factory.CreateClient();
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
            var claims = new[]
            {
                new Claim("CustomerId", _testCustomer.CustomerId.ToString())
            };

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
}
