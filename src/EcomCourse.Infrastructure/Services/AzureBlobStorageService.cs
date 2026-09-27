using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using Microsoft.Extensions.Configuration;

namespace EcomCourse.Infrastructure.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private const long _maxSize = 5 * 1024 * 1024;

        private static readonly Dictionary<string, string> _allowedImageTypes = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/webp", ".webp" },
        };

        private static readonly HashSet<string> _allowedExtensions = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];

            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "product-images";

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<Result<string>> UploadAsync(
            Stream content,
            string contentType,
            string fileName,
            CancellationToken cancellationToken
        )
        {
            if (content is null || content.Length == 0)
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.EmptyFile",
                        "Cannot upload an empty file.",
                        ErrorType.Validation
                    )
                );
            }

            if (content.Length > _maxSize)
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.FileTooLarge",
                        $"File size exceeds the limit of {_maxSize / (1024 * 1024)} MB",
                        ErrorType.Validation
                    )
                );
            }

            var normalizedContentType = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!_allowedImageTypes.ContainsKey(normalizedContentType))
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.InvalidContentType",
                        "Only JPEG, PNG, and WebP image types are supported",
                        ErrorType.Validation
                    )
                );
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension))
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.InvalidFileExtension",
                        "File extension does not match allowed image formats",
                        ErrorType.Validation
                    )
                );
            }
            try
            {
                await _containerClient.CreateIfNotExistsAsync(
                    PublicAccessType.None,
                    cancellationToken: cancellationToken
                );

                if (content.CanSeek && content.Position > 0)
                {
                    content.Position = 0;
                }

                var safeFileName = Path.GetFileName(fileName);
                var blobName = $"{Guid.NewGuid()}-{safeFileName}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(
                    content,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    },
                    cancellationToken
                );

                return Result.Success(blobName);
            }
            catch (RequestFailedException)
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.UploadFailed",
                        "Failed to upload the file to storage.",
                        ErrorType.Failure
                    )
                );
            }
        }

        public async Task<Result> DeleteAsync(string blobName, CancellationToken cancellationToken)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: cancellationToken
                );

                return Result.Success();
            }
            catch (RequestFailedException)
            {
                return Result.Failure(
                    new DomainError(
                        "Storage.DeleteFailed",
                        "Failed to delete the file from storage.",
                        ErrorType.Failure
                    )
                );
            }
        }

        public Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.SasNotSupported",
                        "Storage configuration does not support SAS URL generation",
                        ErrorType.Failure
                    )
                );
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn),
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var url = blobClient.GenerateSasUri(sasBuilder).ToString();
            return Result.Success(url);
        }
    }
}
