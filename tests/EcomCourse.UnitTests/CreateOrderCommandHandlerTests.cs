using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly ICustomerStore _customerStoreMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _customerStoreMock = Substitute.For<ICustomerStore>();
        _handler = new CreateOrderCommandHandler(_orderRepositoryMock, _customerStoreMock);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            Guid.NewGuid(),
            "Test Customer",
            $"{Guid.NewGuid()}@example.com",
            "Khreshchatyk St 1",
            "Kyiv",
            "01001",
            "Ukraine").Value!;
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
    public async Task Handle_ShouldReturnFailure_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest> { new(Guid.NewGuid(), 1, 10m) });

        _customerStoreMock.GetByIdAsync(command.customerId, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NotFound, result.Error);

        await _orderRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderAndSaveToRepository_WhenCommandIsValid()
    {
        // Arrange
        var customer = CreateCustomer();
        var command = new CreateOrderCommand(
            customer.Id,
            new List<OrderLineItemRequest>
            {
                new(Guid.NewGuid(), 2, 100m),
                new(Guid.NewGuid(), 1, 50m)
            });

        _customerStoreMock.GetByIdAsync(command.customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

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
                    o.ShippingAddress.Street == customer.Address.Street),
                Arg.Any<CancellationToken>());
    }
}
