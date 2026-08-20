
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories
{
    public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
}
