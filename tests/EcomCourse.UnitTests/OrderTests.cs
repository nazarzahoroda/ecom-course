using EcomCourse.Domain.Orders;

namespace EcomCourse.UnitTests.Domain.Orders;

public class OrderTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenItemsListIsEmpty()
    {
        var customerId = Guid.NewGuid();
        var emptyItems = Array.Empty<(Guid ProductId, int Quantity, decimal UnitPrice)>();

        // Act
        var result = Order.Create(customerId, emptyItems);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.EmptyLines, result.Error);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidItems = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 0, UnitPrice: 100m)
        };

        // Act
        var result = Order.Create(customerId, invalidItems);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidQuantity, result.Error);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUnitPriceIsNegative()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidItems = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 2, UnitPrice: -50m)
        };

        // Act
        var result = Order.Create(customerId, invalidItems);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidUnitPrice, result.Error);
    }

    [Fact]
    public void Create_ShouldCalculateTotalCorrectly_WhenMultipleItemsProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 2, UnitPrice: 100m),
            (ProductId: Guid.NewGuid(), Quantity: 3, UnitPrice: 50m),
            (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 25.5m)
        };
        const decimal expectedTotal = 375.5m;

        // Act
        var result = Order.Create(customerId, items);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedTotal, result.Value.Total);
        Assert.Equal(3, result.Value.Lines.Count);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
    }

    [Fact]
    public void Lines_ShouldBeReadOnlyCollection_AndContainCorrectData()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var items = new[] { (ProductId: productId, Quantity: 2, UnitPrice: 150m) };

        var result = Order.Create(customerId, items);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var order = result.Value!;


        Assert.IsAssignableFrom<IReadOnlyCollection<OrderLine>>(order.Lines);
        var line = Assert.Single(order.Lines);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(150m, line.UnitPrice);
    }
}
