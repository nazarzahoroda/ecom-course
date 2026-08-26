using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name)
    : IRequest<Result>;
}
