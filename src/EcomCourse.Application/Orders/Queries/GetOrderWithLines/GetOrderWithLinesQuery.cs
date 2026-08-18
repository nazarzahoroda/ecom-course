using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Orders.Queries.GetOrderWithLines;

public record GetOrderWithLinesQuery(Guid OrderId) : IQuery<OrderResponse>;
