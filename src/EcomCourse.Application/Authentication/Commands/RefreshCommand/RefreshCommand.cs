using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;

namespace EcomCourse.Application.Authentication.Commands.RefreshCommand
{
    public record RefreshCommand(string refreshToken) : ICommand<AuthResponse>;
}
