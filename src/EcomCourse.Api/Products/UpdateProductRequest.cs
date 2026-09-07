using EcomCourse.Domain.Products;

namespace EcomCourse.Api.Products;

public sealed record UpdateProductRequest(
    string Name,
    decimal Amount,
    Currency Currency,
    string SKU,
    Guid CategoryId);
