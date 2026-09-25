using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Commands.Create;

public sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryManager _categoryManager;

    public CreateCategoryCommandHandler(
        ICategoryManager categoryManager)
    {
        _categoryManager = categoryManager;
    }

    public async Task<Result<Guid>> Handle(
    CreateCategoryCommand request,
    CancellationToken cancellationToken)
    {
        return await _categoryManager.CreateAsync(
            request.Name,
            cancellationToken);
    }
}

