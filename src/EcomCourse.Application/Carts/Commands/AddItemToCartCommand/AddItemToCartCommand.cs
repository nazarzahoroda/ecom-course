using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;
using MediatR;

namespace EcomCourse.Application.Carts.Commands.AddItemToCartCommand
{
    public record AddItemToCartCommand(Guid customerId, AddItemToCartDto dto) : ICommand;
}
