using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.LogoutCommand
{
    public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IIdentityService _identityService;
        public LogoutCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var revokeResult = await _identityService.RevokeRefreshToken(request.refreshToken, cancellationToken);

            if (revokeResult.IsFailure)
                return revokeResult;
            return revokeResult;
        }
    }
}
