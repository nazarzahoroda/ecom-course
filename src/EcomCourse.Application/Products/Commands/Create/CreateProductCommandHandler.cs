using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.Create;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductManager _productManager;

    public CreateProductCommandHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productManager.CreateAsync(
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId,
            cancellationToken);
    }
}
