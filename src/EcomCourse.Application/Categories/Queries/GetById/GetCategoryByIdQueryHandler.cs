using Microsoft.EntityFrameworkCore;
using EcomCourse.Application.Abstractions.Persistence;
using EcomCourse.Domain;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories.Queries.GetById
{
    public sealed class GetCategoryByIdQueryHandler
     : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCategoryByIdQueryHandler(
            IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> Handle(
            GetCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (category is null)
            {
                return Result.Failure<CategoryDto>(
                    CategoryErrors.NotFound(request.Id));
            }

            var dto = new CategoryDto(
                category.Id,
                category.Name);

            return Result.Success(dto);
        }
    }
}
