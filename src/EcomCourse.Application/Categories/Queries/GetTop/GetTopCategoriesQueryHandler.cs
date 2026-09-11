using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetTop;

public sealed class GetTopCategoriesQueryHandler
    : IQueryHandler<GetTopCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public GetTopCategoriesQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetTopCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryService.GetTopAsync(cancellationToken);
    }
}
