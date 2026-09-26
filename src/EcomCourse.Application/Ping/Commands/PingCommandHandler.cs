using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Ping.Commands;

public sealed class PingCommandHandler : ICommandHandler<PingCommand, string>
{
    public Task<Result<string>> Handle(
        PingCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success("pong"));
    }
}
