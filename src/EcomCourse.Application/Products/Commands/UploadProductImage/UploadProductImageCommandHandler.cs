using System.Security.AccessControl;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Commands.UploadProductImage
{
    public class UploadProductImageCommandHandler : ICommandHandler<UploadProductImageCommand, Guid>
    {
        private readonly IProductService _productService;
        private readonly IBlobStorageService _blobStorage;

        public UploadProductImageCommandHandler(
            IProductService productService,
            IBlobStorageService blobStorage
        )
        {
            _productService = productService;
            _blobStorage = blobStorage;
        }

        public async Task<Result<Guid>> Handle(
            UploadProductImageCommand request,
            CancellationToken cancellationToken
        )
        {
            var isExists = await _productService.IsProductExists(
                request.productId,
                cancellationToken
            );
            if (!isExists)
                return Result.Failure<Guid>(ProductErrors.NotFound(request.productId));

            var blobName = await _blobStorage.UploadAsync(
                request.fileStream,
                request.contentType,
                request.fileName,
                cancellationToken
            );
            if (blobName.IsFailure)
                return Result.Failure<Guid>(blobName.Error);

            if (request.isMain)
            {
                await _productService.ChangeMainImages(request.productId, cancellationToken);
            }

            var imageAddResult = await _productService.AddImage(
                request.productId,
                blobName.Value!,
                request.contentType,
                request.isMain,
                cancellationToken
            );
            return Result.Success(imageAddResult.Value);
        }
    }
}
