using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Queries.GetById;

public sealed record GetCategoryByIdQuery(Guid Id)
    : IQuery<CategoryDto>;
