using EcomCourse.Domain;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Categories
{
    public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly ApplicationDbContext _context;

        public UpdateCategoryCommandHandler(ApplicationDbContext context)   
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)  
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                    
            if (category is null)
            {
                return Result.Failure(CategoryErrors.NotFound(request.Id));    
            }

            var result = category.UpdateName(request.Name);

            if (result.IsFailure)
            {
                return result;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}


