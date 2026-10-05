using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Categories;
using EcomCourse.Domain;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services
{
    public sealed class CategoryManager : ICategoryManager
    {
        private readonly EcomCourseDbContext _dbContext;
        private readonly IBlobStorageService _storageService;

        public CategoryManager(EcomCourseDbContext dbContext, IBlobStorageService storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        public async Task<Result<Guid>> CreateAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            var categoryResult = Category.Create(name);

            if (categoryResult.IsFailure)
            {
                return Result.Failure<Guid>(categoryResult.Error);
            }

            var category = categoryResult.Value!;

            var nameExists = await _dbContext
                .Categories.AsNoTracking()
                .AnyAsync(
                    existingCategory => existingCategory.Name == category.Name,
                    cancellationToken
                );

            if (nameExists)
            {
                return Result.Failure<Guid>(CategoryErrors.NameAlreadyExists);
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

                return Result.Failure<Guid>(CategoryErrors.NameAlreadyExists);
            }
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var category = await _dbContext
                .Categories.AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

            if (category is null)
            {
                return Result.Failure<CategoryDto>(CategoryErrors.NotFound(id));
            }

            var dto = new CategoryDto(category.Id, category.Name, GetImageUrl(category.BlobName));

            return Result.Success(dto);
        }

        public async Task<Result<List<CategoryDto>>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            var categories = await _dbContext
                .Categories.AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.BlobName,
                })
                .ToListAsync(cancellationToken);
            var dtos = categories
                .Select(c => new CategoryDto(c.Id, c.Name, GetImageUrl(c.BlobName)))
                .ToList();
            return Result.Success(dtos);
        }

        public async Task<Result<List<CategoryDto>>> GetTopAsync(
            CancellationToken cancellationToken = default
        )
        {
            var categories = await _dbContext
                .Categories.AsNoTracking()
                .Take(4)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.BlobName,
                })
                .ToListAsync(cancellationToken);

            var dtos = categories
                .Select(c => new CategoryDto(c.Id, c.Name, GetImageUrl(c.BlobName)))
                .ToList();

            return Result.Success(dtos);
        }

        private string? GetImageUrl(string? blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            var sasResult = _storageService.GenerateReadSasUri(blobName, TimeSpan.FromHours(2));

            return sasResult.IsSuccess ? sasResult.Value : null;
        }

        public async Task<Result> UpdateAsync(
            Guid id,
            string name,
            CancellationToken cancellationToken = default
        )
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(
                category => category.Id == id,
                cancellationToken
            );

            if (category is null)
            {
                return Result.Failure(CategoryErrors.NotFound(id));
            }

            var categoryResult = Category.Create(name);

            if (categoryResult.IsFailure)
            {
                return Result.Failure(categoryResult.Error);
            }

            var normalizedName = categoryResult.Value!.Name;

            var nameExists = await _dbContext
                .Categories.AsNoTracking()
                .AnyAsync(
                    existingCategory =>
                        existingCategory.Id != id && existingCategory.Name == normalizedName,
                    cancellationToken
                );

            if (nameExists)
            {
                return Result.Failure(CategoryErrors.NameAlreadyExists);
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
                await _dbContext.Entry(category).ReloadAsync(cancellationToken);

                return Result.Failure(CategoryErrors.NameAlreadyExists);
            }
        }

        public async Task<Result> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(
                category => category.Id == id,
                cancellationToken
            );

            if (category is null)
            {
                return Result.Failure(CategoryErrors.NotFound(id));
            }
            var blobToDelete = category.BlobName;
            _dbContext.Categories.Remove(category);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(blobToDelete))
            {
                await _storageService.DeleteAsync(blobToDelete, CancellationToken.None);
            }
            return Result.Success();
        }

        public async Task<bool> CategoryExists(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories.AnyAsync(x => x.Id == id, cancellationToken);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
        }

        public async Task<Result> AddImageAsync(
            Guid categoryId,
            string blobName,
            CancellationToken cancellationToken
        )
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(
                p => p.Id == categoryId,
                cancellationToken
            );

            if (category is null)
                return Result.Failure(CategoryErrors.NotFound(categoryId));

            if (!string.IsNullOrWhiteSpace(category.BlobName))
            {
                await _storageService.DeleteAsync(category.BlobName, cancellationToken);
            }

            var addResult = category.UpdateImage(blobName);

            if (addResult.IsFailure)
                return Result.Failure(addResult.Error);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> DeleteImageAsync(
            Guid categoryId,
            CancellationToken cancellationToken
        )
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(
                p => p.Id == categoryId,
                cancellationToken
            );

            if (category is null)
                return Result.Failure(CategoryErrors.NotFound(categoryId));

            if (string.IsNullOrWhiteSpace(category.BlobName))
                return Result.Success();

            var deleteResult = await _storageService.DeleteAsync(
                category.BlobName,
                cancellationToken
            );

            if (deleteResult.IsFailure)
                return Result.Failure(deleteResult.Error);

            category.RemoveImage();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
