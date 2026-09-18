using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Queries.GetAll;

public sealed record GetProductsQuery(
    string? Name = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null)
    : IQuery<IReadOnlyList<ProductDto>>;
