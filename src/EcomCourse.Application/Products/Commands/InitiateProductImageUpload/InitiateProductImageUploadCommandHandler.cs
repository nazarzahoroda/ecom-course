using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Commands.InitiateProductImageUpload
{
    public class InitiateProductImageUploadCommandHandler
        : ICommandHandler<InitiateProductImageUploadCommand, InitiateUploadResponse>
    {
        private readonly IProductService _productService;
        private readonly IBlobStorageService _storageService;

        private static readonly HashSet<string> _allowedExtensions = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };

        public InitiateProductImageUploadCommandHandler(
            IProductService productService,
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
            if (string.IsNullOrWhiteSpace(ext) || !_allowedExtensions.Contains(ext))
                return Result.Failure<InitiateUploadResponse>(
                    new DomainError(
                        "Storage.InvalidExtension",
                        "Unsupported file type",
                        ErrorType.Validation
                    )
                );

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
