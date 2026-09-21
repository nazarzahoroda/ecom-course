using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Queries.GetProductImages
{
    public class GetProductImagesQueryHandler
        : IQueryHandler<GetProductImagesQuery, List<ProductImageDto>>
    {
        private readonly IProductService _productService;
        private readonly IBlobStorageService _blobStorageService;

        public GetProductImagesQueryHandler(
            IProductService productService,
            IBlobStorageService blobStorageService
        )
        {
            _productService = productService;
            _blobStorageService = blobStorageService;
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
