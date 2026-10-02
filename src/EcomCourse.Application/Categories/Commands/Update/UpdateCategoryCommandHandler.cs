using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed class UpdateCategoryCommandHandler
    : ICommandHandler<UpdateCategoryCommand>
    {
        private readonly ICategoryManager _categoryManager;

        public UpdateCategoryCommandHandler(
            ICategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        public async Task<Result> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            return await _categoryManager.UpdateAsync(
                request.Id,
                request.Name,
                cancellationToken);
        }
    }
}
