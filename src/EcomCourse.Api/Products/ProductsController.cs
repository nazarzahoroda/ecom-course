using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Commands.Create;
using EcomCourse.Application.Products.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Products;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId);

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

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = result.Value },
            result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);

        var result = await _sender.Send(
            query,
            cancellationToken);

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
