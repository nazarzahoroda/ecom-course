using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public sealed class Product
{
    private readonly List<ProductImage> _images = new();

    private Product() { }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public Price Price { get; private set; } = null!;

    public SKU SKU { get; private set; } = null!;

    public Guid CategoryId { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private Product(Guid id, string name, Price price, SKU sku, Guid categoryId)
    {
        Id = id;
        Name = name;
        Price = price;
        SKU = sku;
        CategoryId = categoryId;
    }

    public static Result<Product> Create(
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(ProductErrors.ProductNameEmpty);
        }

        if (name.Length > 100)
        {
            return Result.Failure<Product>(ProductErrors.ProductNameTooLong);
        }

        var priceResult = Price.Create(amount, currency);

        if (priceResult.IsFailure)
        {
            return Result.Failure<Product>(priceResult.Error);
        }

        var skuResult = SKU.Create(sku);

        if (skuResult.IsFailure)
        {
            return Result.Failure<Product>(skuResult.Error);
        }

        if (categoryId == Guid.Empty)
        {
            return Result.Failure<Product>(ProductErrors.CategoryIdEmpty);
        }

        var product = new Product(
            Guid.NewGuid(),
            name.Trim(),
            priceResult.Value!,
            skuResult.Value!,
            categoryId
        );

        return Result.Success(product);
    }

    public Result Update(
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ProductErrors.ProductNameEmpty);

        if (name.Length > 100)
            return Result.Failure(ProductErrors.ProductNameTooLong);

        var priceResult = Price.Create(amount, currency);
        if (priceResult.IsFailure)
            return Result.Failure(priceResult.Error);

        var skuResult = SKU.Create(sku);
        if (skuResult.IsFailure)
            return Result.Failure(skuResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure(ProductErrors.CategoryIdEmpty);

        Name = name.Trim();
        Price = priceResult.Value!;
        SKU = skuResult.Value!;
        CategoryId = categoryId;

        return Result.Success();
    }

    public Result<ProductImage> AddImage(string blobName, string contentType, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return Result.Failure<ProductImage>(ProductErrors.ImageBlobNameEmpty);
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure<ProductImage>(ProductErrors.ImageContentTypeEmpty);
        }

        if (_images.Count >= 10)
        {
            return Result.Failure<ProductImage>(ProductErrors.MaxImagesLimitReached);
        }

        if (isMain)
        {
            foreach (var img in _images.Where(i => i.IsMain))
            {
                img.SetMain(false);
            }
        }
        else if (_images.Count == 0)
        {
            isMain = true;
        }

        var image = ProductImage.Create(Id, blobName.Trim(), contentType.Trim(), isMain);
        _images.Add(image);

        return Result.Success(image);
    }

    public Result RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return Result.Failure(ProductErrors.ImageNotFound(imageId));
        }

        _images.Remove(image);

        if (image.IsMain && _images.Count > 0)
        {
            _images[0].SetMain(true);
        }

        return Result.Success();
    }
}
