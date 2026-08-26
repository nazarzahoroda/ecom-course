using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Delete
{
    public sealed record DeleteCategoryCommand(Guid Id)
    : IRequest<Result>;
}
