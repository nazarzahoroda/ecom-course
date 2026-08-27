using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Create;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
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

