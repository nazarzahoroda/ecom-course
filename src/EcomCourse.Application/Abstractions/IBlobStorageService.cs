using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Abstractions
{
    public interface IBlobStorageService
    {
        Task<Result> DeleteAsync(string blobName, CancellationToken cancellationToken);
        Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn);
        Result<string> GenerateWriteSasUri(string blobName, string contentType, TimeSpan expiresIn);
        Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken);
    }
}
