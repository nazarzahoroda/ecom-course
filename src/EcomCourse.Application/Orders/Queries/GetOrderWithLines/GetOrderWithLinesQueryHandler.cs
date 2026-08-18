using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Queries.GetOrderWithLines;

public sealed class GetOrderWithLinesQueryHandler : IQueryHandler<GetOrderWithLinesQuery, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderWithLinesQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderWithLinesQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        }

        var linesResponse = order.Lines
            .Select(line => new OrderLineResponse(
                line.Id,
                line.ProductId,
                line.Quantity,
                line.UnitPrice,
                line.Quantity * line.UnitPrice))
            .ToList();

        var response = new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Total,
            linesResponse);

        return Result.Success(response);
    }
}
