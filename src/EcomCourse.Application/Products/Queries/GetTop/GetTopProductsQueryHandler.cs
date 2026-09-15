using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetTop;

public sealed class GetTopProductsQueryHandler
    : IQueryHandler<GetTopProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductService _productService;

    public GetTopProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        GetTopProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productService.GetTopAsync(cancellationToken);
    }
}
