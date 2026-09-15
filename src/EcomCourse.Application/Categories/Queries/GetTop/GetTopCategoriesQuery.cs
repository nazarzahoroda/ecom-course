using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Queries.GetTop;

public sealed record GetTopCategoriesQuery()
    : IQuery<List<CategoryDto>>;
