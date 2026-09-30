using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetProductImages
{
    public class GetProductImagesQueryHandler
        : IQueryHandler<GetProductImagesQuery, List<ProductImageDto>>
    {
        private readonly IProductService _productService;

        public GetProductImagesQueryHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<Result<List<ProductImageDto>>> Handle(
            GetProductImagesQuery request,
            CancellationToken cancellationToken
        )
        {
            var imagesResult = await _productService.GetProductImagesAsync(
                request.productId,
                cancellationToken
            );
            return imagesResult;
        }
    }
}
