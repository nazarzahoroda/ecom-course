using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.Create;

public sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryService _categoryService;

    public CreateCategoryCommandHandler(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<Guid>> Handle(
    CreateCategoryCommand request,
    CancellationToken cancellationToken)
    {
        return await _categoryService.CreateAsync(
            request.Name,
            cancellationToken);
    }
}

