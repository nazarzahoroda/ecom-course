using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Orders;

public class OrderLine : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Виправлено: додано : base(Guid.Empty)
    private OrderLine() : base(Guid.Empty) { }

    private OrderLine(Guid id, Guid productId, int quantity, decimal unitPrice) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<OrderLine> Create(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            return Result.Failure<OrderLine>(OrderErrors.InvalidQuantity);
        }

        if (unitPrice < 0)
        {
            return Result.Failure<OrderLine>(OrderErrors.InvalidUnitPrice);
        }

        return Result.Success(new OrderLine(Guid.NewGuid(), productId, quantity, unitPrice));
    }
}