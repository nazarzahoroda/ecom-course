using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application
{
    internal sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Result<Guid>>,
      IRequestHandler<CreateCategoryCommand, Result<Result<Guid>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken = default)
        {
            if (request.ParentId is not null)
            {
                var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result.Failure<Guid>(CategoryErrors.NotFound(request.ParentId.Value));
                }
            }

            var createResult = Category.Create(request.Name, request.ParentId);
            if (createResult.IsFailure)
            {
                return Result.Failure<Guid>(createResult.Error);
            }

            var category = createResult.Value!;

            _categoryRepository.Add(category!);
            await _categoryRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(category.Id);
        }

        async Task<Result<Result<Guid>>> IRequestHandler<CreateCategoryCommand, Result<Result<Guid>>>.Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = await Handle(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result.Failure<Result<Guid>>(result.Error);
            }

            return Result.Success(result);
        }
    }
}
