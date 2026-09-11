using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Queries.GetTop;

public sealed record GetTopProductsQuery()
    : IQuery<IReadOnlyList<ProductDto>>;
