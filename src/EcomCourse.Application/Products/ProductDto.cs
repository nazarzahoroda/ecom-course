using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Amount,
    Currency Currency,
    string SKU,
    Guid CategoryId);
