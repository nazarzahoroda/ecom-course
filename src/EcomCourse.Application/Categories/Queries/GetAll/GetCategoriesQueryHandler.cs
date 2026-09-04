using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetAll;

public sealed class GetCategoriesQueryHandler
: IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public GetCategoriesQueryHandler(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryService.GetAllAsync(
            cancellationToken);
    }
}
