using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.ConfirmCategoryImageUpload
{
    public class ConfirmCategoryImageUploadCommandHandler
        : ICommandHandler<ConfirmCategoryImageUploadCommand>
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IBlobStorageService _storageService;

        public ConfirmCategoryImageUploadCommandHandler(
            ICategoryManager categoryManager,
            IBlobStorageService storageService
        )
        {
            _categoryManager = categoryManager;
            _storageService = storageService;
        }

        public async Task<Result> Handle(
            ConfirmCategoryImageUploadCommand request,
            CancellationToken cancellationToken
        )
        {
            var exists = await _storageService.ExistsAsync(request.blobName, cancellationToken);
            if (!exists)
            {
                return Result.Failure(
                    new DomainError(
                        "Storage.BlobNotFound",
                        "The image was not uploaded to storage",
                        ErrorType.NotFound
                    )
                );
            }

            try
            {
                var addResult = await _categoryManager.AddImageAsync(
                    request.categoryId,
                    request.blobName,
                    cancellationToken
                );

                if (addResult.IsFailure)
                {
                    await _storageService.DeleteAsync(request.blobName, CancellationToken.None);
                    return Result.Failure(addResult.Error);
                }

                return Result.Success();
            }
            catch
            {
                await _storageService.DeleteAsync(request.blobName, CancellationToken.None);
                throw;
            }
        }
    }
}
