using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.Delete;

public sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductManager _productManager;

    public DeleteProductCommandHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productManager.DeleteAsync(
            request.Id,
            cancellationToken);
    }
}
