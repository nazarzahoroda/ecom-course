using EcomCourse.Application.Abstractions.Messaging;
namespace EcomCourse.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : ICommand;
