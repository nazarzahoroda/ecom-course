using EcomCourse.Domain;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace EcomCourse.Application.Categories
{
    public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetCategoryByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)  
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (category is null)
            {
                return Result.Failure<CategoryDto>(CategoryErrors.NotFound(request.Id));    
            }

            return Result.Success<CategoryDto>(
                new CategoryDto(
                    category.Id,
                    category.Name));
        }
    }
}
