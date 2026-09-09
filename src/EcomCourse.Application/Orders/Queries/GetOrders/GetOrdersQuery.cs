using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    Guid CustomerId,
    int Page,
    int PageSize) : IQuery<GetOrdersResponse>;
