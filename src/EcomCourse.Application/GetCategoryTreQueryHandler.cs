using EcomCourse.Application;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using MediatR;
internal sealed class GetCategoryTreeQueryHandler
    : IQueryHandler<GetCategoryTreeQuery, List<CategoryTreeItemDto>>,
      IRequestHandler<GetCategoryTreeQuery, List<CategoryTreeItemDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryTreeItemDto>> Handle(GetCategoryTreeQuery query, CancellationToken cancellationToken = default)
    {
        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        return BuildTree(allCategories, parentId: null);
    }

    private static List<CategoryTreeItemDto> BuildTree(List<Category> allCategories, Guid? parentId)
    {
        return allCategories
            .Where(category => category.ParentId == parentId)
            .OrderBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
            .Select(category => new CategoryTreeItemDto
            {
                Id = category.Id,
                Name = category.Name,
                Children = BuildTree(allCategories, category.Id),
            })
            .ToList();
    }

    Task<Result<List<CategoryTreeItemDto>>> IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryTreeItemDto>>>.Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
