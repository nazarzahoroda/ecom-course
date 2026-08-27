using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Delete
{
    public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, Result>
    {
       
        private readonly ICategoryService _categoryService;

        public DeleteCategoryCommandHandler(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<Result> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            return await _categoryService.DeleteAsync(
                request.Id,
                cancellationToken);
        }
    }
}
