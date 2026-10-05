using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.DeleteCategoryImage
{
    public class DeleteCategoryImageCommandHandler : ICommandHandler<DeleteCategoryImageCommand>
    {
        private readonly ICategoryManager _categoryManager;

        public DeleteCategoryImageCommandHandler(ICategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        public async Task<Result> Handle(
            DeleteCategoryImageCommand request,
            CancellationToken cancellationToken
        )
        {
            var deleteResult = await _categoryManager.DeleteImageAsync(
                request.categoryId,
                cancellationToken
            );
            return deleteResult;
        }
    }
}
