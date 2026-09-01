using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Commands.Update;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name)
    : ICommand;
