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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(result.Value);
    }
}
