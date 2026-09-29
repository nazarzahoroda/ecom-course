using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<Result> DeleteAsync(string blobName, CancellationToken cancellationToken);
        Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn);

        public Result<string> GenerateWriteSasUri(
            string blobName,
            string contentType,
            TimeSpan expiresIn
        );

        public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken);
    }
}
