using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.Delete
{
    public sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryManager _categoryManager;

        public DeleteCategoryCommandHandler(
            ICategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        public async Task<Result> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            return await _categoryManager.DeleteAsync(
                request.Id,
                cancellationToken);
        }
    }
}
