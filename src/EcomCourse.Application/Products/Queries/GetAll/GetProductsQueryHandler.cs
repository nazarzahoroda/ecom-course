using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetAll;

public sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productService.GetAllAsync(cancellationToken);
    }
}
