using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Authentication.Commands.LogoutCommand
{
    public record LogoutCommand(string refreshToken) : ICommand;
}
