using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Queries.GetAll
{
    public sealed class GetCategoriesQueryHandler
        : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
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
}
