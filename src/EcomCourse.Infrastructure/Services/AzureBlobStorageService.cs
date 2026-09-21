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
            CancellationToken ct
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

            try
            {
                await _containerClient.CreateIfNotExistsAsync(
                    PublicAccessType.None,
                    cancellationToken: ct
                );

                var safeFileName = Path.GetFileName(fileName);
                var blobName = $"{Guid.NewGuid()}-{safeFileName}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(
                    content,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    },
                    ct
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

        public async Task<Result> DeleteAsync(string blobName, CancellationToken ct)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: ct
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
