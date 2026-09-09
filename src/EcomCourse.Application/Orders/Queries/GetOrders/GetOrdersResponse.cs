using EcomCourse.Application.Orders.Queries.GetOrderWithLines;

namespace EcomCourse.Application.Orders.Queries.GetOrders;

public record GetOrdersResponse(
    List<OrderResponse> Orders,
    int TotalCount,
    int Page,
    int PageSize);
