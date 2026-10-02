using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Commands.InitiateProductImageUpload
{
    public record InitiateProductImageUploadCommand(
        Guid ProductId,
        string FileName,
        string ContentType
    ) : ICommand<InitiateUploadResponse>;
}
