using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IProductService _productServiceMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _productServiceMock = Substitute.For<IProductService>();
        _handler = new CreateOrderCommandHandler(_orderRepositoryMock, _productServiceMock);
    }

    private void MockProductPrice(Guid productId, decimal amount)
    {
        var dto = new ProductDto(productId, "Test product", amount, Currency.USD, "SKU-1", Guid.NewGuid());
        _productServiceMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(dto));
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
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();

        MockProductPrice(firstProductId, 100m);
        MockProductPrice(secondProductId, 50m);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest>
            {
                new(firstProductId, 2, 999m),
                new(secondProductId, 1, 999m)
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
                    o.Total == 250m),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_AndIgnoreClientSuppliedPrice_WhenProductDoesNotExist()
    {
        // Arrange
        var missingProductId = Guid.NewGuid();

        _productServiceMock.GetByIdAsync(missingProductId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ProductDto>(ProductErrors.NotFound(missingProductId)));

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest>
            {
                new(missingProductId, 1, 1m)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound(missingProductId), result.Error);

        await _orderRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
