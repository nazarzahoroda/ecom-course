using MediatR;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Categories;

namespace EcomCourse.Application.Categories
{
    public sealed record CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = Category.Create(request.Name);

            if (result.IsFailure)
            {
                return Result.Failure<Guid>(result.Error);
            }

            var category = result.Value!;

            return Result.Success<Guid>(category.Id);
        }
    }
}
