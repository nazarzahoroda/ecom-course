using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetTree;

public sealed class GetCategoryTreeQueryHandler
    : IQueryHandler<GetCategoryTreeQuery, List<CategoryTreeDto>>
{
    private readonly ICategoryService _categoryService;

    public GetCategoryTreeQueryHandler(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryTreeDto>>> Handle(
        GetCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryService.GetTreeAsync(
            cancellationToken);
    }
}
