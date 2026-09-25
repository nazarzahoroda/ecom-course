using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Abstractions.Authentication;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Authentication.Commands.LoginCommand
{
    public class LoginCommandHandler: ICommandHandler<LoginCommand, AuthResponse>
    {
        private readonly IIdentityProvider _identityProvider;
        private readonly ITokenIssuer _tokenIssuer;
        public LoginCommandHandler(IIdentityProvider identityProvider, ITokenIssuer tokenIssuer)
        {
            _identityProvider = identityProvider;
            _tokenIssuer = tokenIssuer;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityProvider.GetUserAsync(request.dto.Email, cancellationToken);
            if (user.IsFailure)
                return Result.Failure<AuthResponse>(user.Error);

            var checkResult = await _identityProvider.CheckPasswordSignInAsync(request.dto, cancellationToken);
            if (checkResult.IsFailure)
                return Result.Failure<AuthResponse>(checkResult.Error);

            var roles = await _identityProvider.GetRolesAsync(request.dto.Email, cancellationToken);

            var details = new UserTokenDetails
            {
                UserId = user.Value!.Id,
                Email = user.Value.Email!,
                CustomerId = user.Value.CustomerId,
                Roles = roles!
            };
            var accessToken = _tokenIssuer.GenerateAccessToken(details);

            var refreshToken = _tokenIssuer.GenerateRefreshToken();

            var refreshTokenSave = await _identityProvider.SaveRefreshToken(refreshToken, user.Value.Id, cancellationToken);
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
