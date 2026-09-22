using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;
using EcomCourse.Domain.Products;

namespace EcomCourse.Domain.Orders;

public class Order : Entity<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(line => line.Quantity * line.UnitPrice);

    public Currency Currency => _lines.Count > 0 ? _lines[0].Currency : default;

    private Order() : base(Guid.Empty) { }

    private Order(Guid id, Guid customerId, List<OrderLine> lines, DateTimeOffset createdAt)
        : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        _lines = lines;
        CreatedAt = createdAt;
    }

    public static Result<Order> Create(
        Guid customerId,
        IReadOnlyCollection<(Guid ProductId, int Quantity, decimal UnitPrice, Currency Currency)> items,
        DateTimeOffset createdAt)
    {
        if (items is null || items.Count == 0)
        {
            return Result.Failure<Order>(OrderErrors.EmptyLines);
        }

        var lines = new List<OrderLine>();

        foreach (var item in items)
        {
            var lineResult = OrderLine.Create(item.ProductId, item.Quantity, item.UnitPrice, item.Currency);
            if (lineResult.IsFailure)
            {
                return Result.Failure<Order>(lineResult.Error);
            }

            lines.Add(lineResult.Value!);
        }

        if (lines.Select(line => line.Currency).Distinct().Count() > 1)
        {
            return Result.Failure<Order>(OrderErrors.MixedCurrencies);
        }

        var order = new Order(Guid.NewGuid(), customerId, lines, createdAt);

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
