using EcomCourse.Application.Categories;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services
{
    public sealed class CategoryManager : ICategoryManager
    {
        private readonly EcomCourseDbContext _dbContext;

        public CategoryManager(EcomCourseDbContext dbContext)
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

            var nameExists = await _dbContext.Categories
                .AsNoTracking()
                .AnyAsync(
                    existingCategory => existingCategory.Name == category.Name,
                    cancellationToken);

            if (nameExists)
            {
                return Result.Failure<Guid>(
                    CategoryErrors.NameAlreadyExists);
            }

            _dbContext.Categories.Add(category);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success(category.Id);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                _dbContext.Entry(category).State = EntityState.Detached;

                return Result.Failure<Guid>(
                    CategoryErrors.NameAlreadyExists);
            }
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

        public async Task<Result<List<CategoryDto>>> GetTopAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Take(4)
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

            var categoryResult = Category.Create(name);

            if (categoryResult.IsFailure)
            {
                return Result.Failure(categoryResult.Error);
            }

            var normalizedName = categoryResult.Value!.Name;

            var nameExists = await _dbContext.Categories
                .AsNoTracking()
                .AnyAsync(
                    existingCategory =>
                        existingCategory.Id != id &&
                        existingCategory.Name == normalizedName,
                    cancellationToken);

            if (nameExists)
            {
                return Result.Failure(
                    CategoryErrors.NameAlreadyExists);
            }

            var updateResult = category.UpdateName(normalizedName);

            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                await _dbContext.Entry(category)
                    .ReloadAsync(cancellationToken);

                return Result.Failure(
                    CategoryErrors.NameAlreadyExists);
            }
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

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && sqlException.Errors
                    .Cast<SqlError>()
                    .Any(error => error.Number is 2601 or 2627);
        }
    }
}
