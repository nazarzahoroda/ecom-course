using EcomCourse.Application.Abstractions.Persistence;
using EcomCourse.Domain;
using Microsoft.EntityFrameworkCore;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Commands.Delete
{
    public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCategoryCommandHandler(
            IApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<Result> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

            if (category is null)
            {
                return Result.Failure(
                    CategoryErrors.NotFound(request.Id));
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();

        }
    }
}
