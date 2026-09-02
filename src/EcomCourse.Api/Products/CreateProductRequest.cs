using EcomCourse.Domain.Products;

namespace EcomCourse.Api.Products;

public sealed record CreateProductRequest(
    string Name,
    decimal Amount,
    Currency Currency,
    string SKU,
    Guid CategoryId);
