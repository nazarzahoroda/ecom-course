using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.RefreshCommand
{
    public class RefreshCommandHandler : ICommandHandler<RefreshCommand, AuthResponse>
    {
        private readonly IIdentityProvider _identityProvider;
        public RefreshCommandHandler(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
        }

        public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var refreshResult = await _identityProvider.CheckRefreshToken(request.refreshToken, cancellationToken);

            return refreshResult;
        }
    }
}
