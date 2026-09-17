using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Queries.GetTree;

public sealed record GetCategoryTreeQuery
    : IQuery<List<CategoryTreeDto>>;
