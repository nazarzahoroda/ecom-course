using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetAll;

public sealed class GetCategoriesQueryHandler
: IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryManager _categoryManager;

    public GetCategoriesQueryHandler(
        ICategoryManager categoryManager)
    {
        _categoryManager = categoryManager;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryManager.GetAllAsync(
            cancellationToken);
    }
}
