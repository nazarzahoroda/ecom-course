using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, GetOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetByCustomerIdAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var orderResponses = orders
            .Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.Status.ToString(),
                order.Total,
                order.Lines
                    .Select(line => new OrderLineResponse(
                        line.Id,
                        line.ProductId,
                        line.Quantity,
                        line.UnitPrice,
                        line.Quantity * line.UnitPrice))
                    .ToList()))
            .ToList();

        var response = new GetOrdersResponse(
            orderResponses,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(response);
    }
}
