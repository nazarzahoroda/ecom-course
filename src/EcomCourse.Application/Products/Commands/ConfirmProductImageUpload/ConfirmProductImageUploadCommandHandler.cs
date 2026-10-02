using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.ConfirmProductImageUpload
{
    public class ConfirmProductImageUploadCommandHandler
        : ICommandHandler<ConfirmProductImageUploadCommand, Guid>
    {
        private readonly IProductService _productService;
        private readonly IBlobStorageService _storageService;

        public ConfirmProductImageUploadCommandHandler(
            IProductService productService,
            IBlobStorageService storageService
        )
        {
            _productService = productService;
            _storageService = storageService;
        }

        public async Task<Result<Guid>> Handle(
            ConfirmProductImageUploadCommand request,
            CancellationToken cancellationToken
        )
        {
            var exists = await _storageService.ExistsAsync(request.BlobName, cancellationToken);
            if (!exists)
            {
                return Result.Failure<Guid>(
                    new DomainError(
                        "Storage.BlobNotFound",
                        "The image was not uploaded to storage",
                        ErrorType.NotFound
                    )
                );
            }

            try
            {
                var addResult = await _productService.AddImage(
                    request.ProductId,
                    request.BlobName,
                    request.ContentType,
                    request.IsMain,
                    cancellationToken
                );

                if (addResult.IsFailure)
                {
                    await _storageService.DeleteAsync(request.BlobName, CancellationToken.None);
                    return Result.Failure<Guid>(addResult.Error);
                }

                return Result.Success(addResult.Value);
            }
            catch
            {
                await _storageService.DeleteAsync(request.BlobName, CancellationToken.None);
                throw;
            }
        }
    }
}
