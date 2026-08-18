using EcomCourse.Application.Abstractions.Messaging;
using MediatR;

namespace EcomCourse.Application
{
    public sealed record GetCategoryTreeQuery
    : IQuery<List<CategoryTreeItemDto>>, IRequest<List<CategoryTreeItemDto>>;
}
