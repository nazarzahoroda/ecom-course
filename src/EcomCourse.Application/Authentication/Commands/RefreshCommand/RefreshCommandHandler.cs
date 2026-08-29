using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.RefreshCommand
{
    public class RefreshCommandHandler : ICommandHandler<RefreshCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        public RefreshCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var refreshResult = await _identityService.CheckRefreshToken(request.refreshToken, cancellationToken);
            if (refreshResult.IsFailure)
                return refreshResult;

            return refreshResult;
        }
    }
}
