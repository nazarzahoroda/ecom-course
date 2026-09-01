using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Commands.Create;

public sealed record CreateCategoryCommand(string Name)
    : ICommand<Guid>;
