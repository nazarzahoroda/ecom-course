using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Commands.InitiateProductImageUpload
{
    public class InitiateProductImageUploadCommandHandler
        : ICommandHandler<InitiateProductImageUploadCommand, InitiateUploadResponse>
    {
        private readonly IProductManager _productService;
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

        public InitiateProductImageUploadCommandHandler(
            IProductManager productService,
            IBlobStorageService storageService
        )
        {
            _productService = productService;
            _storageService = storageService;
        }

        public async Task<Result<InitiateUploadResponse>> Handle(
            InitiateProductImageUploadCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!await _productService.IsProductExists(request.ProductId, cancellationToken))
                return Result.Failure<InitiateUploadResponse>(
                    ProductErrors.NotFound(request.ProductId)
                );

            var ext = Path.GetExtension(request.FileName);
            if (
                string.IsNullOrWhiteSpace(ext)
                || !_allowedMimeTypes.TryGetValue(ext, out var allowedContentTypes)
                || !allowedContentTypes.Contains(
                    request.ContentType,
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

            var safeFileName = Path.GetFileName(request.FileName);
            var blobName = $"{Guid.NewGuid()}-{safeFileName}";

            var sasResult = _storageService.GenerateWriteSasUri(
                blobName,
                request.ContentType,
                TimeSpan.FromMinutes(15)
            );
            if (sasResult.IsFailure)
                return Result.Failure<InitiateUploadResponse>(sasResult.Error);

            return Result.Success(new InitiateUploadResponse(sasResult.Value!, blobName));
        }
    }
}
