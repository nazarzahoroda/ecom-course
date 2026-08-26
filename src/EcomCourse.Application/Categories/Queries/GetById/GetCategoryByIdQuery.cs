using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Queries.GetById
{
    public sealed record GetCategoryByIdQuery(Guid Id)
        : IRequest<Result<CategoryDto>>;

}
