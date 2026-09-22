using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests.Domain.Orders;

public class OrderTests
{
    private static readonly DateTimeOffset FixedCreatedAt = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ShouldReturnFailure_WhenItemsListIsEmpty()
    {
        var customerId = Guid.NewGuid();
        var emptyItems = Array.Empty<(Guid ProductId, int Quantity, decimal UnitPrice, Currency Currency)>();

        // Act
        var result = Order.Create(customerId, emptyItems, FixedCreatedAt);

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
            (ProductId: Guid.NewGuid(), Quantity: 0, UnitPrice: 100m, Currency: Currency.USD)
        };

        // Act
        var result = Order.Create(customerId, invalidItems, FixedCreatedAt);

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
            (ProductId: Guid.NewGuid(), Quantity: 2, UnitPrice: -50m, Currency: Currency.USD)
        };

        // Act
        var result = Order.Create(customerId, invalidItems, FixedCreatedAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidUnitPrice, result.Error);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCurrencyIsInvalid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidItems = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 100m, Currency: (Currency)999)
        };

        // Act
        var result = Order.Create(customerId, invalidItems, FixedCreatedAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidCurrency, result.Error);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenLinesUseDifferentCurrencies()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 100m, Currency: Currency.USD),
            (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 100m, Currency: Currency.UAH)
        };

        // Act
        var result = Order.Create(customerId, items, FixedCreatedAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.MixedCurrencies, result.Error);
    }

    [Fact]
    public void Create_ShouldCalculateTotalCorrectly_WhenMultipleItemsProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new[]
        {
            (ProductId: Guid.NewGuid(), Quantity: 2, UnitPrice: 100m, Currency: Currency.USD),
            (ProductId: Guid.NewGuid(), Quantity: 3, UnitPrice: 50m, Currency: Currency.USD),
            (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 25.5m, Currency: Currency.USD)
        };
        const decimal expectedTotal = 375.5m;

        // Act
        var result = Order.Create(customerId, items, FixedCreatedAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedTotal, result.Value.Total);
        Assert.Equal(3, result.Value.Lines.Count);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
        Assert.Equal(Currency.USD, result.Value.Currency);
        Assert.Equal(FixedCreatedAt, result.Value.CreatedAt);
    }

    [Fact]
    public void Lines_ShouldBeReadOnlyCollection_AndContainCorrectData()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var items = new[] { (ProductId: productId, Quantity: 2, UnitPrice: 150m, Currency: Currency.USD) };

        var result = Order.Create(customerId, items, FixedCreatedAt);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var order = result.Value!;


        Assert.IsAssignableFrom<IReadOnlyCollection<OrderLine>>(order.Lines);
        var line = Assert.Single(order.Lines);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(150m, line.UnitPrice);
        Assert.Equal(Currency.USD, line.Currency);
    }

    private static Order CreatePendingOrder()
    {
        var result = Order.Create(
            Guid.NewGuid(),
            new[] { (ProductId: Guid.NewGuid(), Quantity: 1, UnitPrice: 10m, Currency: Currency.USD) },
            FixedCreatedAt);

        return result.Value!;
    }

    [Fact]
    public void MarkAsPaid_ShouldSetStatusToPaid_WhenOrderIsPending()
    {
        var order = CreatePendingOrder();

        var result = order.MarkAsPaid();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenOrderIsPending()
    {
        var order = CreatePendingOrder();

        var result = order.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void MarkAsPaid_ShouldReturnFailure_WhenOrderIsAlreadyPaid()
    {
        var order = CreatePendingOrder();
        order.MarkAsPaid();

        var result = order.MarkAsPaid();

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Cancel_ShouldReturnFailure_WhenOrderIsAlreadyCancelled()
    {
        var order = CreatePendingOrder();
        order.Cancel();

        var result = order.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ShouldReturnFailure_WhenOrderIsPaid()
    {
        var order = CreatePendingOrder();
        order.MarkAsPaid();

        var result = order.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void MarkAsPaid_ShouldReturnFailure_WhenOrderIsCancelled()
    {
        var order = CreatePendingOrder();
        order.Cancel();

        var result = order.MarkAsPaid();

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
