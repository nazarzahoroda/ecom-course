using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Commands.Delete;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
