using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EcomCourse.Application.Orders.Commands.CancelOrder;
using EcomCourse.Application.Orders.Commands.MarkOrderAsPaid;

namespace EcomCourse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderWithLinesQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == OrderErrors.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkOrderAsPaid(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkOrderAsPaidCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == OrderErrors.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelOrderCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == OrderErrors.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }
}
