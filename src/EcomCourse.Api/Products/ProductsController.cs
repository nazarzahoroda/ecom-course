using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Commands.Create;
using EcomCourse.Application.Products.Commands.Delete;
using EcomCourse.Application.Products.Commands.Update;
using EcomCourse.Application.Products.Queries.GetAll;
using EcomCourse.Application.Products.Queries.GetById;
using EcomCourse.Application.Products.Queries.GetTop;
using EcomCourse.Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId
        );

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }

        return CreatedAtAction(nameof(GetProductById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);

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

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProducts(CancellationToken cancellationToken)
    {
        var query = new GetTopProductsQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId
        );

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == ProductErrors.NotFound(id).Code)
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

            return BadRequest(
                new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);

        var result = await _sender.Send(command, cancellationToken);

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

        return NoContent();
    }
}
