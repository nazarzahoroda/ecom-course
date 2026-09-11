using EcomCourse.Application.Orders.Queries.GetOrders;
using EcomCourse.Domain.Orders;

namespace EcomCourse.UnitTests.Application.Orders.GetOrders;

public class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task HandleReturnsOnlyOrdersOfRequestedCustomer()
    {
        var repository = new FakeOrderRepository();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        var myOrder = CreateTestOrder(customerId);
        var otherOrder = CreateTestOrder(otherCustomerId);

        await repository.AddAsync(myOrder, CancellationToken.None);
        await repository.AddAsync(otherOrder, CancellationToken.None);

        var handler = new GetOrdersQueryHandler(repository);
        var query = new GetOrdersQuery(customerId, Page: 1, PageSize: 10);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Orders);
        Assert.Equal(myOrder.Id, result.Value.Orders[0].Id);
    }

    [Fact]
    public async Task HandleReturnsCorrectPageAndTotalCount()
    {
        var repository = new FakeOrderRepository();
        var customerId = Guid.NewGuid();

        for (var i = 0; i < 5; i++)
        {
            await repository.AddAsync(CreateTestOrder(customerId), CancellationToken.None);
        }

        var handler = new GetOrdersQueryHandler(repository);
        var query = new GetOrdersQuery(customerId, Page: 2, PageSize: 2);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Orders.Count);
        Assert.Equal(5, result.Value.TotalCount);
    }

    private static Order CreateTestOrder(Guid customerId)
    {
        var items = new List<(Guid ProductId, int Quantity, decimal UnitPrice)>
        {
            (Guid.NewGuid(), 1, 100m)
        };

        return Order.Create(customerId, items).Value!;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = [];

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
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
            var filtered = _orders.Where(o => o.CustomerId == customerId).ToList();
            var totalCount = filtered.Count;

            var paged = filtered
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<(IReadOnlyList<Order>, int)>((paged, totalCount));
        }
    }
}
