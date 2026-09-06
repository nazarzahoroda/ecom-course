using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Commands.Create;

public sealed record CreateProductCommand(
    string Name,
    decimal Amount,
    Currency Currency,
    string SKU,
    Guid CategoryId) : ICommand<Guid>;
