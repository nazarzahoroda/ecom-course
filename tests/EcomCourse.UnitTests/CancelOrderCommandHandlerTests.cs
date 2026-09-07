using EcomCourse.Application.Orders.Commands.CancelOrder;
using EcomCourse.Domain.Orders;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _handler = new CancelOrderCommandHandler(_orderRepositoryMock);
    }

    private static Order CreatePendingOrder()
    {
        var result = Order.Create(
            Guid.NewGuid(),
            new[] { (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 10m) });

        return result.Value!;
    }

    [Fact]
    public async Task Handle_ShouldCancelAndUpdate_WhenOrderIsPending()
    {
        var order = CreatePendingOrder();
        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        await _orderRepositoryMock.Received(1)
            .UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var command = new CancelOrderCommand(Guid.NewGuid());

        _orderRepositoryMock.GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.NotFound, result.Error);
        await _orderRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTransitionIsInvalid()
    {
        var order = CreatePendingOrder();
        order.MarkAsPaid();

        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
        await _orderRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
