using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Domain.Orders;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
        _handler = new CreateOrderCommandHandler(_orderRepositoryMock, _timeProvider);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenItemsListIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest>());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.EmptyLines, result.Error);

        await _orderRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderAndSaveToRepository_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest>
            {
                new(Guid.NewGuid(), 2, 100m),
                new(Guid.NewGuid(), 1, 50m)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _orderRepositoryMock.Received(1)
            .AddAsync(
                Arg.Is<Order>(o =>
                    o.Id == result.Value &&
                    o.CustomerId == command.customerId &&
                    o.Total == 250m &&
                    o.CreatedAt == _timeProvider.GetUtcNow()),
                Arg.Any<CancellationToken>());
    }
}
