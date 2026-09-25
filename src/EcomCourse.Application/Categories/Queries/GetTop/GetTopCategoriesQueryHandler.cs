using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Queries.GetTop;

public sealed class GetTopCategoriesQueryHandler
    : IQueryHandler<GetTopCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryManager _categoryManager;

    public GetTopCategoriesQueryHandler(ICategoryManager categoryManager)
    {
        _categoryManager = categoryManager;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetTopCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _categoryManager.GetTopAsync(cancellationToken);
    }
}
