using EcomCourse.Api.Customers;
using EcomCourse.Application.Customers.GetCustomerById;
using EcomCourse.Application.Customers.RegisterCustomer;
using EcomCourse.Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers;

[ApiController]
[Route("customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCustomer(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.UserId,
            request.Name,
            request.Email,
            request.Street,
            request.City,
            request.PostalCode,
            request.Country);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var problemDetails = new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description
            };

            if (result.Error == CustomerErrors.EmailAlreadyExists)
            {
                problemDetails.Status = StatusCodes.Status409Conflict;

                return Conflict(problemDetails);
            }

            problemDetails.Status = StatusCodes.Status400BadRequest;

            return BadRequest(problemDetails);
        }

        return Created($"/customers/{result.Value}", result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result.Value);
    }
}
