using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly EcomCourseDbContext _dbContext;

    public ProductService(EcomCourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> CreateAsync(
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(category => category.Id == categoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<Guid>(ProductErrors.CategoryNotFound(categoryId));
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(product => product.SKU.Value == sku, cancellationToken);

        if (skuExists)
        {
            return Result.Failure<Guid>(ProductErrors.SKUAlreadyExists(sku));
        }

        var productResult = Product.Create(
            name,
            amount,
            currency,
            sku,
            categoryId);

        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        var product = productResult.Value!;

            _dbContext.Products.Add(product);

            await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.Id == id,
                cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDto>(
                ProductErrors.NotFound(id));
        }

        var dto = new ProductDto(
            product.Id,
            product.Name,
            product.Price.Amount,
            product.Price.Currency,
            product.SKU.Value,
            product.CategoryId);

        return Result.Success(dto);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)  
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Price.Amount,
                product.Price.Currency,
                product.SKU.Value,
                product.CategoryId))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductDto>>(products);
    }
}
