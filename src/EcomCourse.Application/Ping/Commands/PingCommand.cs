using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Ping.Commands;

public sealed record PingCommand : ICommand<string>;
