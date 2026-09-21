using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public record OrderLineItemRequest(
    Guid ProductId,
    int Quantity);

public record CreateOrderCommand(
    Guid customerId,
    List<OrderLineItemRequest> items) : ICommand<Guid>;
