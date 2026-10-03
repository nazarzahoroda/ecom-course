using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcomCourse.Infrastructure.Services
{
    public partial class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger
        )
        {
            _logger = logger;

            var connectionString = configuration["AzureBlobStorage:ConnectionString"];

            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "product-images";

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
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
            catch (RequestFailedException ex)
            {
                LogStorageOperationFailed(
                    _logger,
                    ex,
                    blobName,
                    ex.Status,
                    ex.ErrorCode ?? "Unknown"
                );

                return Result.Failure(
                    new DomainError(
                        ex.ErrorCode ?? "Storage.OperationFailed",
                        "Failed to perform storage operation",
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

        public Result<string> GenerateWriteSasUri(
            string blobName,
            string contentType,
            TimeSpan expiresIn
        )
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                return Result.Failure<string>(
                    new DomainError(
                        "Storage.SasNotSupported",
                        "Cannot generate SAS URL",
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
                ContentType = contentType,
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var uri = blobClient.GenerateSasUri(sasBuilder).ToString();
            return Result.Success(uri);
        }

        public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync(cancellationToken);
        }

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Azure Blob Storage operation failed for {BlobName}. Status: {Status}, ErrorCode: {ErrorCode}"
        )]
        private static partial void LogStorageOperationFailed(
            ILogger logger,
            Exception exception,
            string blobName,
            int status,
            string errorCode
        );
    }
}
