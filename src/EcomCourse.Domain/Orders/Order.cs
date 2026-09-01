using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Orders;

public class Order : Entity<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(line => line.Quantity * line.UnitPrice);

    private Order() : base(Guid.Empty) { }

    private Order(Guid id, Guid customerId, List<OrderLine> lines)
        : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        _lines = lines;
    }

    public static Result<Order> Create(
        Guid customerId,
        IReadOnlyCollection<(Guid ProductId, int Quantity, decimal UnitPrice)> items)
    {
        if (items is null || items.Count == 0)
        {
            return Result.Failure<Order>(OrderErrors.EmptyLines);
        }

        var lines = new List<OrderLine>();

        foreach (var item in items)
        {
            var lineResult = OrderLine.Create(item.ProductId, item.Quantity, item.UnitPrice);
            if (lineResult.IsFailure)
            {
                return Result.Failure<Order>(lineResult.Error);
            }

            lines.Add(lineResult.Value!);
        }

        var order = new Order(Guid.NewGuid(), customerId, lines);

        return Result.Success(order);
    }

    public Result MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }
        Status = OrderStatus.Paid;
        return Result.Success();
    }
    public Result Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }
        Status = OrderStatus.Cancelled;
        return Result.Success();
    }
}
