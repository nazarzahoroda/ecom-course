using EcomCourse.Application.Authentication.Commands.LoginCommand;
using EcomCourse.Application.Authentication.Commands.LogoutCommand;
using EcomCourse.Application.Authentication.Commands.RefreshCommand;
using EcomCourse.Application.Authentication.Commands.RegisterCommand;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Authentication.Interfaces;
using EcomCourse.Domain.Common;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
        {
            var request = new RegisterCommand(dto);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }
                return StatusCode(StatusCodes.Status201Created);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _manager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized();

            var userDto = new ApplicationUserDto
            {
                Id = user.Id,
                Email = user.Email,
                CustomerId = user.CustomerId
            };

            var request = new LoginCommand(dto, userDto);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            return Ok(result.Value);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(string refreshToken, CancellationToken cancellationToken)
        {
            var request = new RefreshCommand(refreshToken);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            return Ok(result.Value);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string refreshToken, CancellationToken cancellationToken)
        {
            var request = new LogoutCommand(refreshToken);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            return Ok();
        }
    }
}
