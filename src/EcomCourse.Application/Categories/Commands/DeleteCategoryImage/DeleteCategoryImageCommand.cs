using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Commands.DeleteCategoryImage
{
    public record DeleteCategoryImageCommand(Guid categoryId) : ICommand;
}
