using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;
using MediatR;


namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly ICategoryService _categoryService;

        public UpdateCategoryCommandHandler(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<Result> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            return await _categoryService.UpdateAsync(
                request.Id,
                request.Name,
                cancellationToken);
        }
    }
}
