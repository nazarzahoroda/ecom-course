using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Authentication.Interfaces;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.LoginCommand
{
    public class LoginCommandHandler: ICommandHandler<LoginCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService)
        {
            _identityService = identityService;
            _jwtService = jwtService;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.GetUserAsync(request.dto.Email, cancellationToken);
            if (user.IsFailure)
                return Result.Failure<AuthResponse>(user.Error);

            var checkResult = await _identityService.CheckPasswordSignInAsync(request.dto, cancellationToken);
            if (checkResult.IsFailure)
                return Result.Failure<AuthResponse>(checkResult.Error);

            var roles = await _identityService.GetRolesAsync(request.dto.Email, cancellationToken);

            var details = new UserTokenDetails
            {
                UserId = user.Value!.Id,
                Email = user.Value.Email!,
                CustomerId = user.Value.CustomerId,
                Roles = roles!
            };
            var accessToken = _jwtService.GenerateAccessToken(details);

            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenSave = await _identityService.SaveRefreshToken(refreshToken, user.Value.Id, cancellationToken);
            if (refreshTokenSave.IsFailure)
                return Result.Failure<AuthResponse>(refreshTokenSave.Error);

            var result = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Result.Success(result);

        }
    }
}
