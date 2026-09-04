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


        public AuthController(ISender sender)
        {
            _sender = sender;
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
            var request = new LoginCommand(dto);
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
        public async Task<IActionResult> Refresh(RefreshDto dto, CancellationToken cancellationToken)
        {
            var request = new RefreshCommand(dto.RefreshToken);
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
        public async Task<IActionResult> Logout(LogoutDto dto, CancellationToken cancellationToken)
        {
            var request = new LogoutCommand(dto.RefreshToken);
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
