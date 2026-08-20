
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories
{
    public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result>; 
}
