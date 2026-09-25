using EcomCourse.Application.Authentication.DTOs;

namespace EcomCourse.Application.Abstractions.Authentication
{
    public interface ITokenIssuer
    {
        string GenerateAccessToken(UserTokenDetails details);
        string GenerateRefreshToken();
    }
}
