using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Create
{
    public sealed record CreateCategoryCommand(string Name)
    : IRequest<Result<Guid>>;
}
