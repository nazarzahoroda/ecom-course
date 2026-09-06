using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Queries.GetAll;

public sealed record GetProductsQuery()
    : IQuery<IReadOnlyList<ProductDto>>;
