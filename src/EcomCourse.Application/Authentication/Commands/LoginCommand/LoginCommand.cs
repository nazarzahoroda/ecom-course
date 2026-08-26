using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;

namespace EcomCourse.Application.Authentication.Commands.LoginCommand
{
    public record LoginCommand() : ICommand<AuthResponse>;
}
