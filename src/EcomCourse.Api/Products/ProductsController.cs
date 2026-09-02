using EcomCourse.Application.Products.Commands.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Products;

[ApiController]
[Route("products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Created(
            $"/products/{result.Value}",
            result.Value);
    }
}
