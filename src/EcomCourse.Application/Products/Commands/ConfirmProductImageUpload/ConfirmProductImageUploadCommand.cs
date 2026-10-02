using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Commands.ConfirmProductImageUpload
{
    public record ConfirmProductImageUploadCommand(
        Guid ProductId,
        string BlobName,
        string ContentType,
        bool IsMain
    ) : ICommand<Guid>;
}
