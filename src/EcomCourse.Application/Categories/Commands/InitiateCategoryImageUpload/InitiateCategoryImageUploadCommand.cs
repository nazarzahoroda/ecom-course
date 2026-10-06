using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products;

namespace EcomCourse.Application.Categories.Commands.InitiateCategoryImageUpload
{
    public record InitiateCategoryImageUploadCommand(
        Guid categoryId,
        string fileName,
        string contentType
    ) : ICommand<InitiateUploadResponse>;
}
