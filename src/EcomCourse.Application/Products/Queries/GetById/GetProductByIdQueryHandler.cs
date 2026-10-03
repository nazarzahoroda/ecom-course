using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetById;

public sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductManager _productManager;

    public GetProductByIdQueryHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _productManager.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}
