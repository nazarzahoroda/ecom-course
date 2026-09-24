using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;
using EcomCourse.Domain.Products;

namespace EcomCourse.Domain.Orders;

public class OrderLine : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public Currency Currency { get; private set; }

    private OrderLine() : base(Guid.Empty) { }

    private OrderLine(Guid id, Guid productId, int quantity, decimal unitPrice, Currency currency) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public static Result<OrderLine> Create(Guid productId, int quantity, decimal unitPrice, Currency currency)
    {
        if (quantity <= 0)
        {
            return Result.Failure<OrderLine>(OrderErrors.InvalidQuantity);
        }

        if (unitPrice < 0)
        {
            return Result.Failure<OrderLine>(OrderErrors.InvalidUnitPrice);
        }

        if (!Enum.IsDefined(currency))
        {
            return Result.Failure<OrderLine>(OrderErrors.InvalidCurrency);
        }

        return Result.Success(new OrderLine(Guid.NewGuid(), productId, quantity, unitPrice, currency));
    }
}
