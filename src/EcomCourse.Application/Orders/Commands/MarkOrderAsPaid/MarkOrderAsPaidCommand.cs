using EcomCourse.Application.Abstractions.Messaging;
namespace EcomCourse.Application.Orders.Commands.MarkOrderAsPaid;

public record MarkOrderAsPaidCommand(Guid OrderId) : ICommand;
