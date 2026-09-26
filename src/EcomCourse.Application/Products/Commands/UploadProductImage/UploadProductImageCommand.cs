using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Commands.UploadProductImage
{
    public record UploadProductImageCommand(
        Guid productId,
        Stream fileStream,
        string fileName,
        string contentType,
        bool isMain
    ) : ICommand<Guid>;
}
