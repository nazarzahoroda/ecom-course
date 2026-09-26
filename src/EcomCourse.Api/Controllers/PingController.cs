using EcomCourse.Application.Ping.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers;

[ApiController]
[Route("ping")]
[Produces("application/json")]
public sealed class PingController : ControllerBase
{
    private readonly ISender _sender;

    public PingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PingCommand(), cancellationToken);

        return Ok(result.Value);
    }
}
