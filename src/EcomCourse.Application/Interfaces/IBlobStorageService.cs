using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<Result<string>> UploadAsync(
            Stream content,
            string contentType,
            string fileName,
            CancellationToken cancellationToken
        );
        Task<Result> DeleteAsync(string blobName, CancellationToken cancellationToken);
        Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn);
    }
}
