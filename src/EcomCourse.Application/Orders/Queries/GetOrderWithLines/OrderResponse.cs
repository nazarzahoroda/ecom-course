using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Orders.Queries.GetOrderWithLines;

public record OrderLineResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    Currency Currency,
    decimal LineTotal);

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal Total,
    Currency Currency,
    List<OrderLineResponse> Lines);
