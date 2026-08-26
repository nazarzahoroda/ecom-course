using EcomCourse.Application.Abstractions.Persistence;
using EcomCourse.Domain;
using EcomCourse.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCategoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _context.Categories
             .FirstOrDefaultAsync(
                 category => category.Id == request.Id,
                 cancellationToken);

            if (category is null)
            {
                return Result.Failure(
                    CategoryErrors.NotFound(request.Id));
            }
            var updateResult = category.UpdateName(request.Name);

            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();

        }
    }
}
