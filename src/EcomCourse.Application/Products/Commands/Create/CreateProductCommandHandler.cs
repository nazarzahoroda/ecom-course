using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.Create;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productService.CreateAsync(
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId,
            cancellationToken);
    }
}
