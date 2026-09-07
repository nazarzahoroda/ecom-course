using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Queries.GetById;

public sealed record GetProductByIdQuery(Guid Id)
    : IQuery<ProductDto>;
