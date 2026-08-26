using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Authentication.Interfaces;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly EcomCourseDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IdentityDbContext _identityContext;


        public AuthController(ISender sender, UserManager<ApplicationUser> manager, EcomCourseDbContext context,
            SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IdentityDbContext identityContext)
        {
            _sender = sender;
            _manager = manager;
            _context = context;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _identityContext = identityContext;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        //{
        //    var existingUser = await _manager.FindByEmailAsync(request.Email);

        //    if (existingUser is not null)
        //    {
        //        return BadRequest("User with this email already exists.");
        //    }

        //    var user = new ApplicationUser
        //    {
        //        UserName = request.Email,
        //        Email = request.Email
        //    };

        //    var createUserResult =
        //        await _manager.CreateAsync(
        //            user,
        //            request.Password);

        //    if (!createUserResult.Succeeded)
        //    {
        //        return BadRequest(
        //            createUserResult.Errors.Select(
        //                x => x.Description));
        //    }

        //    try
        //    {
        //        var customer = new Customer
        //        {
        //            Id = Guid.NewGuid(),
        //            UserId = user.Id
        //        };

        //        _context.Customers.Add(customer);

        //        await _context.SaveChangesAsync(
        //            cancellationToken);

        //        user.CustomerId = customer.Id;

        //        var updateUserResult =
        //            await _manager.UpdateAsync(user);

        //        if (!updateUserResult.Succeeded)
        //        {
        //            await CompensateAsync(
        //                user,
        //                customer.Id,
        //                cancellationToken);

        //            return BadRequest(
        //                updateUserResult.Errors.Select(
        //                    x => x.Description));
        //        }

        //        var roleResult =
        //            await _manager.AddToRoleAsync(
        //                user,
        //                "Customer");

        //        if (!roleResult.Succeeded)
        //        {
        //            await CompensateAsync(
        //                user,
        //                customer.Id,
        //                cancellationToken);

        //            return BadRequest(
        //                roleResult.Errors.Select(
        //                    x => x.Description));
        //        }

        //        return StatusCode(
        //            StatusCodes.Status201Created);
        //    }
        //    catch
        //    {
        //        await CompensateAsync(
        //            user,
        //            user.CustomerId,
        //            cancellationToken);

        //        throw;
        //    }
        //}
    //    private async Task CompensateAsync(
    //ApplicationUser user,
    //Guid customerId,
    //CancellationToken cancellationToken)
    //    {
    //        if (customerId != Guid.Empty)
    //        {
    //            var customer =
    //                await _context.Customers
    //                    .FirstOrDefaultAsync(
    //                        x => x.Id == customerId,
    //                        cancellationToken);

    //            if (customer is not null)
    //            {
    //                _context.Customers.Remove(customer);

    //                await _context.SaveChangesAsync(
    //                    cancellationToken);
    //            }
    //        }

    //        await _manager.DeleteAsync(user);
    //    }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
        {
            var user = await _manager.FindByEmailAsync(
                dto.Email);

            if (user is null)
            {
                return Unauthorized();
            }

            var result =
                await _signInManager.CheckPasswordSignInAsync(
                    user,
                    dto.Password,
                    lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles =
                await _manager.GetRolesAsync(user);

            var details = new UserTokenDetails
            {
                UserId = user.Id,
                Email = user.Email!,
                CustomerId = user.CustomerId,
                Roles = roles
            };

            var accessToken =
                _jwtService.GenerateAccessToken(details);

            var refreshToken =
                _jwtService.GenerateRefreshToken();

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _identityContext.RefreshTokens.Add(entity);

            await _identityContext.SaveChangesAsync();
            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        {
            var refreshToken =
                await _identityContext.RefreshTokens
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(
                        x => x.Token == request.RefreshToken);

            if (refreshToken is null ||
                refreshToken.IsRevoked ||
                refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized();
            }

            var roles =
                await _manager.GetRolesAsync(
                    refreshToken.User);

            var details = new UserTokenDetails
            {
                UserId = refreshToken.User.Id,
                Email = refreshToken.User.Email!,
                CustomerId = refreshToken.User.CustomerId,
                Roles = roles
            };

            var accessToken =
                _jwtService.GenerateAccessToken(details);
            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
            return Ok(response);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            var refreshToken =
                await _identityContext.RefreshTokens
                    .FirstOrDefaultAsync(
                        x => x.Token == request.RefreshToken);

            if (refreshToken is null)
            {
                return Unauthorized();
            }

            refreshToken.IsRevoked = true;

            await _identityContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
