using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;

namespace EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand
{
    public record UpdateCartItemQuantityCommand(UpdateCartItemQuantityDto dto) : ICommand;
}
