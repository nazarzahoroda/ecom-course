using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand
{
public record RemoveItemFromCartCommand(Guid id) : ICommand;
}
