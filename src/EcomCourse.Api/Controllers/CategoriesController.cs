using EcomCourse.Application.Categories.Create;
using EcomCourse.Domain.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Created(
            $"/api/categories/{result.Value}",
            result.Value);
    }
}
