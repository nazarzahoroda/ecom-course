using EcomCourse.Api.Common;
using EcomCourse.Application.Customers.GetCustomerById;
using EcomCourse.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuthorizationService _authorizationService;

    public CustomersController(ISender sender, IAuthorizationService authorizationService)
    {
        _sender = sender;
        _authorizationService = authorizationService;
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
            return result.ToProblemDetails();
        }
        var resource = new CustomerResource(result.Value!.Id);

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            User,
            resource,
            "SameCustomerOrAdmin"
        );

        if (!authorizationResult.Succeeded)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Customer.NotFound",
                    Detail = "Customer was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(result.Value);
    }
}
