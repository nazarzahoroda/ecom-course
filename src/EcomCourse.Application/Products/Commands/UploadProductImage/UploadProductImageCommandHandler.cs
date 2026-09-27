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

            var uploadResult = await _blobStorage.UploadAsync(
                request.fileStream,
                request.contentType,
                request.fileName,
                cancellationToken
            );
            if (uploadResult.IsFailure)
                return Result.Failure<Guid>(uploadResult.Error);

            try
            {
                if (request.isMain)
                {
                    await _productService.ChangeMainImages(request.productId, cancellationToken);
                }

                var imageAddResult = await _productService.AddImage(
                    request.productId,
                    uploadResult.Value!,
                    request.contentType,
                    request.isMain,
                    cancellationToken
                );

                if (imageAddResult.IsFailure)
                {
                    await CompensateUploadedBlobAsync(uploadResult.Value!);
                    return Result.Failure<Guid>(imageAddResult.Error);
                }

                return Result.Success(imageAddResult.Value);
            }
            catch
            {
                await CompensateUploadedBlobAsync(uploadResult.Value!);
                throw;
            }
        }

        private async Task CompensateUploadedBlobAsync(string blobName)
        {
            await _blobStorage.DeleteAsync(blobName, CancellationToken.None);
        }
    }
}
