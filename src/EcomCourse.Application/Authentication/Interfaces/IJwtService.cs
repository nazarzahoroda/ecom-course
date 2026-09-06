using EcomCourse.Application.Authentication.DTOs;

namespace EcomCourse.Application.Authentication.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(UserTokenDetails details);
        string GenerateRefreshToken();
    }
}
