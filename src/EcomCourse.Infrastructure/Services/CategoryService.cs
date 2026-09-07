using EcomCourse.Application.Categories;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Domain;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly EcomCourseDbContext _dbContext;

        public CategoryService(EcomCourseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> CreateAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var categoryResult = Category.Create(name);

            if (categoryResult.IsFailure)
            {
                return Result.Failure<Guid>(
                    categoryResult.Error);
            }

            var category = categoryResult.Value!;

            _dbContext.Categories.Add(category);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(category.Id);
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    category => category.Id == id,
                    cancellationToken);

            if (category is null)
            {
                return Result.Failure<CategoryDto>(
                    CategoryErrors.NotFound(id));
            }

            var dto = new CategoryDto(
                category.Id,
                category.Name);

            return Result.Success(dto);
        }

        public async Task<Result<List<CategoryDto>>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name))
                .ToListAsync(cancellationToken);

            return Result.Success(categories);
        }

        public async Task<Result> UpdateAsync(
            Guid id,
            string name,
            CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(
                    category => category.Id == id,
                    cancellationToken);

            if (category is null)
            {
                return Result.Failure(
                    CategoryErrors.NotFound(id));
            }

            var updateResult = category.UpdateName(name);

            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(
                    category => category.Id == id,
                    cancellationToken);

            if (category is null)
            {
                return Result.Failure(
                    CategoryErrors.NotFound(id));
            }

            _dbContext.Categories.Remove(category);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
