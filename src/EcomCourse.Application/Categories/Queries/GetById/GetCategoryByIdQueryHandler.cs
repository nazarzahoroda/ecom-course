using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetById;

public sealed class GetCategoryByIdQueryHandler
 : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryManager _categoryManager;

    public GetCategoryByIdQueryHandler(
        ICategoryManager categoryManager)
    {
        _categoryManager = categoryManager;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryManager.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}
