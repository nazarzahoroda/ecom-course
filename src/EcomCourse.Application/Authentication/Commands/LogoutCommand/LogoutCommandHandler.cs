using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.LogoutCommand
{
    public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IIdentityProvider _identityProvider;
        public LogoutCommandHandler(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var revokeResult = await _identityProvider.RevokeRefreshToken(request.refreshToken, cancellationToken);
            return revokeResult;
        }
    }
}
