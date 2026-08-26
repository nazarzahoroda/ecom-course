using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Queries.GetAll
{
    public sealed record GetCategoriesQuery()
    : IRequest<Result<List<CategoryDto>>>;
}
