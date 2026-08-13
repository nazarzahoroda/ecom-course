namespace EcomCourse.Application.Orders.Queries.GetOrderWithLines;

public record OrderLineResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal Total,
    List<OrderLineResponse> Lines);