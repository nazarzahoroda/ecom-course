using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Authentication.Interfaces;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EcomCourse.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IdentityDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public IdentityService(
            UserManager<ApplicationUser> manager,
            IdentityDbContext context,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IConfiguration configuration
        )
        {
            _manager = manager;
            _context = context;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        #region sign in
        private async Task<ApplicationUser?> GetUserByEmailAsync(
            string email,
            CancellationToken cancellationToken
        )
        {
            var user = await _manager.FindByEmailAsync(email);
            if (user is null)
                return null;
            return user;
        }

        public async Task<bool> IsUserExist(string email, CancellationToken cancellationToken)
        {
            var existingUser = await _manager.FindByEmailAsync(email);

            if (existingUser is null)
                return false;

            return true;
        }

        public async Task<Result<ApplicationUserDto>> GetUserAsync(
            string email,
            CancellationToken cancellationToken
        )
        {
            var existingUser = await _manager.FindByEmailAsync(email);

            if (existingUser is null)
                return Result.Failure<ApplicationUserDto>(
                    new DomainError("Identity.UserNotFound", "User not found")
                );
            var result = new ApplicationUserDto
            {
                Id = existingUser.Id,
                Email = existingUser.Email,
                CustomerId = existingUser.CustomerId,
            };

            return Result.Success(result);
        }

        public async Task<Result> CreateUserAsync(
            RegisterDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
            var createUserResult = await _manager.CreateAsync(user, dto.Password);
            if (!createUserResult.Succeeded)
            {
                return Result.Failure(
                    new DomainError("Identity.CreateUserFailed", "Failed to create user.")
                );
            }
            return Result.Success(user);
        }

        public async Task<Result<ApplicationUserDto>> CreateUserAsyncWithResult(
            RegisterDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = new ApplicationUser { UserName = dto.Name, Email = dto.Email };

            var createUserResult = await _manager.CreateAsync(user, dto.Password);

            if (!createUserResult.Succeeded)
            {
                var errors = string.Join("; ", createUserResult.Errors.Select(x => x.Description));

                return Result.Failure<ApplicationUserDto>(
                    new DomainError("Identity.CreateUserFailed", errors)
                );
            }
            var roleResult = await _manager.AddToRoleAsync(user, "Customer");

            if (!roleResult.Succeeded)
            {
                var deleteResult = await DeleteUserAsync(user.Id, cancellationToken);

                return Result.Failure<ApplicationUserDto>(
                    new DomainError("Identity.AddRoleFailed", "Failed to add Customer role")
                );
            }
            var result = new ApplicationUserDto { Id = user.Id, Email = user.Email! };

            return Result.Success(result);
        }

        public async Task<Result> SetCustomerIdAsync(
            Guid userId,
            Guid customerId,
            CancellationToken cancellationToken
        )
        {
            var user = await _manager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return Result.Failure(new DomainError("Identity.UserNotFound", "User not found"));
            }

            user.CustomerId = customerId;

            var result = await _manager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Result.Failure(
                    new DomainError("Identity.UpdateUserFailed", "Failed to update user")
                );
            }

            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _manager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return Result.Failure(new DomainError("Identity.UserNotFound", "User not found"));
            }
            var result = await _manager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return Result.Failure(
                    new DomainError("Identity.DeleteUserFailed", "Failed to delete user")
                );
            }
            return Result.Success();
        }

        public async Task<Result> CheckPasswordSignInAsync(
            LoginDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = await GetUserByEmailAsync(dto.Email, cancellationToken);

            if (user is null)
            {
                return Result.Failure(new DomainError("Identity.UserNotFound", "User not found"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                dto.Password!,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                return Result.Failure(
                    new DomainError("Identity.InvalidCredentials", "Invalid credentials")
                );
            }

            return Result.Success();
        }

        public async Task<IList<string>?> GetRolesAsync(
            string email,
            CancellationToken cancellationToken
        )
        {
            var user = await GetUserByEmailAsync(email, cancellationToken);

            if (user is null)
                return null;

            return await _manager.GetRolesAsync(user);
        }
        #endregion
        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToHexString(hashBytes);
        }

        public async Task<Result> SaveRefreshToken(
            string refreshToken,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var days = _configuration.GetValue<double>("Jwt:RefreshTokenDays");
            var tokenHash = HashToken(refreshToken);
            var entity = new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                CreatedAt = DateTime.UtcNow,
            };

            _context.RefreshTokens.Add(entity);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Result.Failure(
                    new DomainError("Identity.RefreshToken", "Failed to save refresh token")
                );
            }
            return Result.Success();
        }

        private async Task<RefreshToken?> GetRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            var tokenHash = HashToken(refreshToken);

            var token = await _context
                .RefreshTokens.Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

            return token;
        }

        public async Task<Result<AuthResponse>> CheckRefreshToken(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            var token = await GetRefreshTokenAsync(refreshToken, cancellationToken);

            if (token is null)
                return Result.Failure<AuthResponse>(
                    new DomainError("Identity.RefreshToken", "Refresh token not found in data base")
                );

            if (token.IsRevoked)
            {
                var userTokens = await _context
                    .RefreshTokens.Where(t => t.UserId == token.UserId && t.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var t in userTokens)
                {
                    t.RevokedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Failure<AuthResponse>(
                    new DomainError(
                        "Identity.RefreshTokenCompromised",
                        "A token reuse attempt was detected. All sessions have been terminated."
                    )
                );
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
                return Result.Failure<AuthResponse>(
                    new DomainError("Identity.RefreshToken", "Refresh token expired")
                );

            token.RevokedAt = DateTime.UtcNow;

            var roles = await _manager.GetRolesAsync(token.User);

            var details = new UserTokenDetails
            {
                UserId = token.User.Id,
                Email = token.User.Email!,
                CustomerId = token.User.CustomerId,
                Roles = roles,
            };

            var accessToken = _jwtService.GenerateAccessToken(details);

            var newRawRefreshToken = _jwtService.GenerateRefreshToken();

            var newRefreshTokenHash = HashToken(newRawRefreshToken);

            token.ReplacedByTokenHash = newRefreshTokenHash;

            var days = _configuration.GetValue<double>("Jwt:RefreshTokenDays");

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = token.UserId,
                TokenHash = newRefreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
            };

            await _context.RefreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRawRefreshToken,
            };

            return Result.Success(response);
        }

        public async Task<Result> RevokeRefreshToken(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            var token = await GetRefreshTokenAsync(refreshToken, cancellationToken);
            if (token is null)
                return Result.Failure(
                    new DomainError("Identity.RefreshToken", "Refresh token not found")
                );

            if (token.IsRevoked)
                return Result.Success();

            token.RevokedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Result.Failure(
                    new DomainError("Identity.RefreshToken", "Failed to revoke refresh token")
                );
            }
            return Result.Success();
        }
    }
}
