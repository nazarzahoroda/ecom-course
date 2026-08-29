using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Interfaces
{
    public interface IIdentityService
    {
        #region sign in
        Task<ApplicationUserDto?> GetUserAsync(string email, CancellationToken cancellationToken);
        Task<bool> IsUserExist(string email, CancellationToken cancellationToken);

        Task<Result> CreateUserAsync(RegisterDto dto, CancellationToken cancellationToken);
        Task<Result<ApplicationUserDto>> CreateUserAsyncWithResult(RegisterDto dto, CancellationToken cancellationToken);
        Task<Result> SetCustomerIdAsync(Guid userId, Guid customerId, CancellationToken cancellationToken);

        Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

        Task<Result> CheckPasswordSignInAsync(LoginDto dto, CancellationToken cancellationToken);

        Task<IList<string>?> GetRolesAsync(string email, CancellationToken cancellationToken);
        #endregion

        Task<Result> SaveRefreshToken(string refreshToken, Guid userId, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> CheckRefreshToken(string refreshToken, CancellationToken cancellationToken);
        Task<Result> RevokeRefreshToken(string refreshToken, CancellationToken cancellationToken);
    }
}
