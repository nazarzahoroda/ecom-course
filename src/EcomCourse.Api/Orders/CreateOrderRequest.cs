using EcomCourse.Application.Orders.Commands.CreateOrder;

namespace EcomCourse.Api.Orders;

public sealed record CreateOrderRequest(List<OrderLineItemRequest> Items);
