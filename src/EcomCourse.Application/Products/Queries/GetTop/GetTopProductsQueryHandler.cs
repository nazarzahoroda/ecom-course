using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetTop;

public sealed class GetTopProductsQueryHandler
    : IQueryHandler<GetTopProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductManager _productManager;

    public GetTopProductsQueryHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        GetTopProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productManager.GetTopAsync(cancellationToken);
    }
}
