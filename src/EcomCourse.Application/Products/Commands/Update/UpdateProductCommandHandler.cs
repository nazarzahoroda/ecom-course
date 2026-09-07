using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.Update;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productService.UpdateAsync(
            request.Id,
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId,
            cancellationToken);
    }
}
