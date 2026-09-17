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
            Guid? parentId = null,
            CancellationToken cancellationToken = default)
        {
            if (parentId.HasValue)
            {
                var parentExists = await _dbContext.Categories
                    .AnyAsync(
                        category => category.Id == parentId.Value,
                        cancellationToken);

                if (!parentExists)
                {
                    return Result.Failure<Guid>(
                        CategoryErrors.NotFound(parentId.Value));
                }
            }

            var categoryResult = Category.Create(
                name,
                parentId);

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

        public async Task<Result<List<CategoryTreeDto>>> GetTreeAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var rootCategories = categories
                .Where(category => category.ParentId is null)
                .Select(category => BuildTree(
                    category,
                    categories))
                .ToList();

            return Result.Success(rootCategories);
        }

        private static CategoryTreeDto BuildTree(
            Category category,
            List<Category> categories)
        {
            var children = categories
                .Where(child => child.ParentId == category.Id)
                .Select(child => BuildTree(
                    child,
                    categories))
                .ToList();

            return new CategoryTreeDto(
                category.Id,
                category.Name,
                children);
        }

        public async Task<Result> UpdateAsync(
            Guid id,
            string name,
            Guid? parentId = null,
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

            if (parentId.HasValue)
            {
                var currentParent = await _dbContext.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        category => category.Id == parentId.Value,
                        cancellationToken);

                if (currentParent is null)
                {
                    return Result.Failure(
                        CategoryErrors.NotFound(parentId.Value));
                }

                while (currentParent is not null)
                {
                    if (currentParent.Id == id)
                    {
                        return Result.Failure(
                            CategoryErrors.CyclicReference);
                    }

                    if (!currentParent.ParentId.HasValue)
                    {
                        break;
                    }

                    currentParent = await _dbContext.Categories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            category => category.Id == currentParent.ParentId.Value,
                            cancellationToken);
                }
            }

            var updateNameResult = category.UpdateName(name);

            if (updateNameResult.IsFailure)
            {
                return Result.Failure(updateNameResult.Error);
            }

            var updateParentResult = category.UpdateParent(parentId);

            if (updateParentResult.IsFailure)
            {
                return Result.Failure(updateParentResult.Error);
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
