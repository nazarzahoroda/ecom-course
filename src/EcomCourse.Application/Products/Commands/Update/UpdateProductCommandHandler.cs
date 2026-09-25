using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.Update;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductManager _productManager;

    public UpdateProductCommandHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productManager.UpdateAsync(
            request.Id,
            request.Name,
            request.Amount,
            request.Currency,
            request.SKU,
            request.CategoryId,
            cancellationToken);
    }
}
