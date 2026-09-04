using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Commands.Delete;

public sealed record DeleteCategoryCommand(Guid Id)
    : ICommand;
