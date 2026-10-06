using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Commands.ConfirmCategoryImageUpload
{
    public record ConfirmCategoryImageUploadCommand(Guid categoryId, string blobName) : ICommand;
}
