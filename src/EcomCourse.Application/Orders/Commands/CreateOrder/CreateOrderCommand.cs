using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public record OrderLineItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);

public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderLineItemRequest> Items) : ICommand<Guid>;