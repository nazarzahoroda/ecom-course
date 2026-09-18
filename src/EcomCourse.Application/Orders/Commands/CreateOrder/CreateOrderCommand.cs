using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public record OrderLineItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    Currency Currency);

public record CreateOrderCommand(
    Guid customerId,
    List<OrderLineItemRequest> items) : ICommand<Guid>;
