using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed class UpdateCategoryCommandHandler
    : ICommandHandler<UpdateCategoryCommand>
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
