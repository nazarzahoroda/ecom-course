using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests;

public class OrderLineTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var productId = Guid.NewGuid();
        var quantity = 2;
        var unitPrice = 100m;
        var currency = Currency.UAH;

        var result = OrderLine.Create(productId, quantity, unitPrice, currency);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal(productId, result.Value.ProductId);
        Assert.Equal(quantity, result.Value.Quantity);
        Assert.Equal(unitPrice, result.Value.UnitPrice);
        Assert.Equal(currency, result.Value.Currency);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldReturnFailure()
    {
        var result = OrderLine.Create(
            Guid.NewGuid(),
            0,
            100m,
            Currency.UAH);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidQuantity, result.Error);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldReturnFailure()
    {
        var result = OrderLine.Create(
            Guid.NewGuid(),
            -1,
            100m,
            Currency.UAH);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidQuantity, result.Error);
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_ShouldReturnFailure()
    {
        var result = OrderLine.Create(
            Guid.NewGuid(),
            1,
            -1m,
            Currency.UAH);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidUnitPrice, result.Error);
    }

    [Fact]
    public void Create_WithInvalidCurrency_ShouldReturnFailure()
    {
        var result = OrderLine.Create(
            Guid.NewGuid(),
            1,
            100m,
            (Currency)999);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidCurrency, result.Error);
    }

    [Fact]
    public void Create_WithZeroUnitPrice_ShouldReturnSuccess()
    {
        var result = OrderLine.Create(
            Guid.NewGuid(),
            1,
            0m,
            Currency.UAH);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.UnitPrice);
    }
}
