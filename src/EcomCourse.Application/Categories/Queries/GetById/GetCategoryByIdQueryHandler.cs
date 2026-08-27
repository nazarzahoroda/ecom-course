using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;
using MediatR;


namespace EcomCourse.Application.Categories.Queries.GetById
{
    public sealed class GetCategoryByIdQueryHandler
     : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly ICategoryService _categoryService;

        public GetCategoryByIdQueryHandler(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<Result<CategoryDto>> Handle(
            GetCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _categoryService.GetByIdAsync(
                request.Id,
                cancellationToken);
        }
    }
}
