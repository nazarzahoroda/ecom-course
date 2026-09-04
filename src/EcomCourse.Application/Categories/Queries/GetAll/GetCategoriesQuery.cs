using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Categories.Queries.GetAll
{
    public sealed record GetCategoriesQuery
    : IQuery<List<CategoryDto>>;
}
