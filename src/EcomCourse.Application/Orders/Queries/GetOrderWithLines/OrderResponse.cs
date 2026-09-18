namespace EcomCourse.Application.Orders.Queries.GetOrderWithLines;

public record OrderLineResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderShippingAddressResponse(
    string Street,
    string City,
    string PostalCode,
    string Country);

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal Total,
    OrderShippingAddressResponse ShippingAddress,
    List<OrderLineResponse> Lines);
