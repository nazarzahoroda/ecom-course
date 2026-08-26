using EcomCourse.Application.Abstractions.Persistence;
using EcomCourse.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Categories.Queries.GetAll
{
    public sealed class GetCategoriesQueryHandler
        : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetCategoriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CategoryDto>>> Handle(
            GetCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name))
                .ToListAsync(cancellationToken);

            return Result.Success(categories);
        }
    }
}
