using EcomCourse.Application.Abstractions.Persistence;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Create;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryResult =
            Category.Create(request.Name);

        if (categoryResult.IsFailure)
        {
            return Result.Failure<Guid>(
                categoryResult.Error);
        }

        var category = categoryResult.Value!;

        _context.Categories.Add(category);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(category.Id);
    }
}

