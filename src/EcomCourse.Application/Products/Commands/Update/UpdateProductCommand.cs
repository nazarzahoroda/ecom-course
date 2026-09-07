using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Commands.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Amount,
    Currency Currency,
    string SKU,
    Guid CategoryId) : ICommand;
