using Azure.Core;
using EcomCourse.Application.Interfaces;
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
    private readonly IBlobStorageService _storageService;

    public ProductService(EcomCourseDbContext dbContext, IBlobStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<Result<Guid>> CreateAsync(
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        var productResult = Product.Create(name, amount, currency, sku, categoryId);

        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        var product = productResult.Value!;

        var categoryExists = await _dbContext.Categories.AnyAsync(
            category => category.Id == product.CategoryId,
            cancellationToken
        );

        if (!categoryExists)
        {
            return Result.Failure<Guid>(ProductErrors.CategoryNotFound(product.CategoryId));
        }

        var skuExists = await _dbContext.Products.AnyAsync(
            existingProduct => existingProduct.SKU.Value == product.SKU.Value,
            cancellationToken
        );

        if (skuExists)
        {
            return Result.Failure<Guid>(ProductErrors.SKUAlreadyExists(product.SKU.Value));
        }

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _dbContext
            .Products.Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDto>(ProductErrors.NotFound(id));

        var imageDtos = product
            .Images.Select(img =>
            {
                var sasResult = _storageService.GenerateReadSasUri(
                    img.BlobName,
                    TimeSpan.FromHours(2)
                );

                return new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = product.Id,
                    Url = sasResult.IsSuccess ? sasResult.Value! : string.Empty,
                    IsMain = img.IsMain,
                };
            })
            .ToList();

        var dto = new ProductDto(
            product.Id,
            product.Name,
            product.Price.Amount,
            product.Price.Currency,
            product.SKU.Value,
            product.CategoryId,
            imageDtos
        );

        return Result.Success(dto);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ToListAsync(cancellationToken);

        var foundIds = products.Select(product => product.Id).ToHashSet();
        var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            return Result.Failure<IReadOnlyList<ProductDto>>(
                ProductErrors.NotFound(missingIds[0]));
        }

        IReadOnlyList<ProductDto> dtos = products
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Price.Amount,
                product.Price.Currency,
                product.SKU.Value,
                product.CategoryId))
            .ToList();

        return Result.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _dbContext
            .Products.Include(p => p.Images)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productDtos = products
            .Select(product =>
            {
                var mainImage =
                    product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();

                var imagesList = new List<ProductImageDto>();

                if (mainImage is not null)
                {
                    var sasResult = _storageService.GenerateReadSasUri(
                        mainImage.BlobName,
                        TimeSpan.FromHours(2)
                    );

                    imagesList.Add(
                        new ProductImageDto
                        {
                            Id = mainImage.Id,
                            ProductId = product.Id,
                            Url = sasResult.IsSuccess ? sasResult.Value! : string.Empty,
                            IsMain = mainImage.IsMain,
                        }
                    );
                }

                return new ProductDto(
                    product.Id,
                    product.Name,
                    product.Price.Amount,
                    product.Price.Currency,
                    product.SKU.Value,
                    product.CategoryId,
                    imagesList
                );
            })
            .ToList();

        return Result.Success<IReadOnlyList<ProductDto>>(productDtos);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetTopAsync(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _dbContext
            .Products.Include(p => p.Images)
            .AsNoTracking()
            .Take(4)
            .ToListAsync(cancellationToken);

        var productDtos = products
            .Select(product =>
            {
                var mainImage =
                    product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();

                var imagesList = new List<ProductImageDto>();

                if (mainImage is not null)
                {
                    var sasResult = _storageService.GenerateReadSasUri(
                        mainImage.BlobName,
                        TimeSpan.FromHours(2)
                    );

                    imagesList.Add(
                        new ProductImageDto
                        {
                            Id = mainImage.Id,
                            ProductId = product.Id,
                            Url = sasResult.IsSuccess ? sasResult.Value! : string.Empty,
                            IsMain = mainImage.IsMain,
                        }
                    );
                }

                return new ProductDto(
                    product.Id,
                    product.Name,
                    product.Price.Amount,
                    product.Price.Currency,
                    product.SKU.Value,
                    product.CategoryId,
                    imagesList
                );
            })
            .ToList();

        return Result.Success<IReadOnlyList<ProductDto>>(productDtos);
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(
            product => product.Id == id,
            cancellationToken
        );

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(id));
        }

        var categoryExists = await _dbContext.Categories.AnyAsync(
            category => category.Id == categoryId,
            cancellationToken
        );

        if (!categoryExists)
        {
            return Result.Failure(ProductErrors.CategoryNotFound(categoryId));
        }

        var skuExists = await _dbContext.Products.AnyAsync(
            product => product.Id != id && product.SKU.Value == sku,
            cancellationToken
        );

        if (skuExists)
        {
            return Result.Failure(ProductErrors.SKUAlreadyExists(sku));
        }

        var updateResult = product.Update(name, amount, currency, sku, categoryId);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(
            product => product.Id == id,
            cancellationToken
        );

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(id));
        }

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<ProductImage?>> GetProductImageAsync(
        Guid imageId,
        Guid productId,
        CancellationToken cancellationToken
    )
    {
        var image = await _dbContext.ProductImages.FirstOrDefaultAsync(
            x => x.Id == imageId && x.ProductId == productId,
            cancellationToken
        );

        if (image is null)
            return Result.Failure<ProductImage?>(ProductErrors.ImageNotFound(imageId));

        return Result.Success<ProductImage?>(image);
    }

    public async Task<bool> IsProductExists(Guid id, CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == id, cancellationToken);
        return productExists;
    }

    public async Task ChangeMainImages(Guid id, CancellationToken cancellationToken)
    {
        var currentMains = await _dbContext
            .ProductImages.Where(x => x.ProductId == id && x.IsMain)
            .ToListAsync(cancellationToken);

        foreach (var img in currentMains)
            img.SetMain(false);
    }

    public async Task<Result<Guid>> AddImage(
        Guid Id,
        string blobName,
        string contentType,
        bool isMain,
        CancellationToken cancellationToken
    )
    {
        var image = ProductImage.Create(Id, blobName, contentType, isMain);

        _dbContext.ProductImages.Add(image);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(image.Id);
    }

    public async Task<Result<List<ProductImageDto>>> GetProductImagesAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var images = await _dbContext
            .ProductImages.AsNoTracking()
            .Where(x => x.ProductId == id)
            .ToListAsync(cancellationToken);

        if (images is null || images.Count == 0)
        {
            return Result.Failure<List<ProductImageDto>>(ProductErrors.ProductImagesNotFound(id));
        }

        var imageDtos = images
            .Select(img =>
            {
                var sasResult = _storageService.GenerateReadSasUri(
                    img.BlobName,
                    TimeSpan.FromHours(1)
                );

                return new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    Url = sasResult.IsSuccess ? sasResult.Value! : string.Empty,
                    IsMain = img.IsMain,
                };
            })
            .ToList();

        return Result.Success(imageDtos);
    }

    public async Task<Result> DeleteImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken
    )
    {
        var imageResult = await GetProductImageAsync(imageId, productId, cancellationToken);

        if (imageResult.IsFailure)
            return Result.Failure(imageResult.Error);

        var image = imageResult.Value!;

        _dbContext.ProductImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var deleteResult = await _storageService.DeleteAsync(image.BlobName, cancellationToken);

        if (deleteResult.IsFailure)
            return Result.Failure(deleteResult.Error);

        return Result.Success();
    }
}
