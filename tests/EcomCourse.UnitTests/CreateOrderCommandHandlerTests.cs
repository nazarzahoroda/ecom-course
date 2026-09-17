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
    private readonly Dictionary<Guid, decimal> _prices = [];

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _productServiceMock = Substitute.For<IProductService>();
        _handler = new CreateOrderCommandHandler(_orderRepositoryMock, _productServiceMock);

        _productServiceMock
            .GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var ids = call.Arg<IReadOnlyCollection<Guid>>();
                var missingId = ids.FirstOrDefault(id => !_prices.ContainsKey(id));

                if (missingId != default)
                {
                    return Result.Failure<IReadOnlyList<ProductDto>>(ProductErrors.NotFound(missingId));
                }

                IReadOnlyList<ProductDto> dtos = ids
                    .Select(id => new ProductDto(id, "Test product", _prices[id], Currency.USD, "SKU-1", Guid.NewGuid()))
                    .ToList();

                return Result.Success(dtos);
            });
    }

    private void MockProductPrice(Guid productId, decimal amount) => _prices[productId] = amount;

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
                new(firstProductId, 2),
                new(secondProductId, 1)
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
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        // Arrange
        var missingProductId = Guid.NewGuid();

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderLineItemRequest>
            {
                new(missingProductId, 1)
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
