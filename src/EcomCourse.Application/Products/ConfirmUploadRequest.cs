namespace EcomCourse.Application.Products
{
    public sealed record ConfirmUploadRequest(string BlobName, string ContentType, bool IsMain);
}
