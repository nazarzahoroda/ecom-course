using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<Result<string>> UploadAsync(
            Stream content,
            string contentType,
            string fileName,
            CancellationToken ct
        );
        Task<Result> DeleteAsync(string blobName, CancellationToken ct);
        Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn);
    }
}
