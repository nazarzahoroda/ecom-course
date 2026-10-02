namespace EcomCourse.Application.Products
{
    public sealed record InitiateUploadRequest(string FileName, string ContentType);

    public sealed record InitiateUploadResponse(string UploadUrl, string BlobName);
}
