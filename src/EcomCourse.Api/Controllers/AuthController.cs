using System.Security.Claims;
using EcomCourse.Api.Common;
using EcomCourse.Application.Authentication.Commands.LoginCommand;
using EcomCourse.Application.Authentication.Commands.LogoutCommand;
using EcomCourse.Application.Authentication.Commands.RefreshCommand;
using EcomCourse.Application.Authentication.Commands.RegisterCommand;
using EcomCourse.Application.Authentication.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterDto dto,
            CancellationToken cancellationToken
        )
        {
            var request = new RegisterCommand(dto);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var request = new LoginCommand(dto);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60),
            };

            Response.Cookies.Append("access_token", result.Value!.AccessToken, cookieOptions);

            Response.Cookies.Append(
                "refresh_token",
                result.Value.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                }
            );
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest();
            }

            var request = new RefreshCommand(refreshToken);
            var result = await _sender.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                Response.Cookies.Delete("access_token");
                Response.Cookies.Delete("refresh_token");
                return result.ToProblemDetails();
            }

            Response.Cookies.Append(
                "access_token",
                result.Value!.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                }
            );
            Response.Cookies.Append(
                "refresh_token",
                result.Value.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                }
            );
            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest();
            }

            var request = new LogoutCommand(refreshToken);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userDetails = new
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = User.FindFirstValue(ClaimTypes.Email),
                CustomerId = User.FindFirstValue("CustomerId"),
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value),
            };

            return Ok(userDetails);
        }
    }
}
