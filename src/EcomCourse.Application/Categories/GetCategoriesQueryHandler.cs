
using EcomCourse.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Categories
{
    public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>  
    {
        private readonly ApplicationDbContext _context;

        public GetCategoriesQueryHandler(ApplicationDbContext context)    
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)   
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new CategoryDto(
                    x.Id,
                    x.Name))
                .ToListAsync(cancellationToken);
        }
    }
}
