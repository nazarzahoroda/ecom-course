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
            var checkResult = await _identityService.CheckPasswordSignInAsync(request.dto, cancellationToken);
            if (checkResult.IsFailure)
                return Result.Failure<AuthResponse>(checkResult.Error);

            var roles = await _identityService.GetRolesAsync(request.dto.Email, cancellationToken);

            var details = new UserTokenDetails
            {
                UserId = request.userDto.Id,
                Email = request.userDto.Email!,
                CustomerId = request.userDto.CustomerId,
                Roles = roles!
            };
            var acceaccessToken = _jwtService.GenerateAccessToken(details);

            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenSave = await _identityService.SaveRefreshToken(refreshToken, request.userDto.Id, cancellationToken);
            if (refreshTokenSave.IsFailure)
                return Result.Failure<AuthResponse>(checkResult.Error);

            var result = new AuthResponse
            {
                AccessToken = acceaccessToken,
                RefreshToken = refreshToken
            };
            return Result.Success(result);

        }
    }
}
