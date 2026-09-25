using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetAll;

public sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductManager _productManager;

    public GetProductsQueryHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productManager.GetAllAsync(cancellationToken);
    }
}
