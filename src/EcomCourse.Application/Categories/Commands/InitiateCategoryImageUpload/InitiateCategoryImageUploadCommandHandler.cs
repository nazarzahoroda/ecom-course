using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products;
using EcomCourse.Domain;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.InitiateCategoryImageUpload
{
    public class InitiateCategoryImageUploadCommandHandler
        : ICommandHandler<InitiateCategoryImageUploadCommand, InitiateUploadResponse>
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IBlobStorageService _storageService;

        private static readonly Dictionary<string, string[]> _allowedMimeTypes = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".webp"] = ["image/webp"],
        };

        public InitiateCategoryImageUploadCommandHandler(
            ICategoryManager categoryManager,
            IBlobStorageService storageService
        )
        {
            _categoryManager = categoryManager;
            _storageService = storageService;
        }

        public async Task<Result<InitiateUploadResponse>> Handle(
            InitiateCategoryImageUploadCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!await _categoryManager.CategoryExists(request.categoryId, cancellationToken))
                return Result.Failure<InitiateUploadResponse>(
                    CategoryErrors.NotFound(request.categoryId)
                );

            var ext = Path.GetExtension(request.fileName);
            if (
                string.IsNullOrWhiteSpace(ext)
                || !_allowedMimeTypes.TryGetValue(ext, out var allowedContentTypes)
                || !allowedContentTypes.Contains(
                    request.contentType,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            {
                return Result.Failure<InitiateUploadResponse>(
                    new DomainError(
                        "Storage.InvalidFileType",
                        "Unsupported file extension or content type.",
                        ErrorType.Validation
                    )
                );
            }

            var safeFileName = Path.GetFileName(request.fileName);
            var blobName =
                $"categories/{request.categoryId}/{Guid.NewGuid()}{ext.ToLowerInvariant()}";

            var sasResult = _storageService.GenerateWriteSasUri(
                blobName,
                request.contentType,
                TimeSpan.FromMinutes(15)
            );
            if (sasResult.IsFailure)
                return Result.Failure<InitiateUploadResponse>(sasResult.Error);

            return Result.Success(new InitiateUploadResponse(sasResult.Value!, blobName));
        }
    }
}
