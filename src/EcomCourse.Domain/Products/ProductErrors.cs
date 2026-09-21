using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public static class ProductErrors
{
    public static readonly DomainError ProductNameEmpty = new(
        "Product.NameEmpty",
        "You need enter name Product.",
        ErrorType.Validation
    );

    public static readonly DomainError ProductNameTooLong = new(
        "Product.NameTooLong",
        "Product name must be contain less 101 characters.",
        ErrorType.Validation
    );

    public static readonly DomainError CategoryIdEmpty = new(
        "Product.CategoryIdEmpty",
        "Category Id cannot be empty.",
        ErrorType.Validation
    );

    public static readonly DomainError Unavailable = new(
        "Product.Unavailable",
        "Product is unavailable",
        ErrorType.Conflict
    );

    public static DomainError CategoryNotFound(Guid categoryId) =>
        new(
            "Product.CategoryNotFound",
            $"Category {categoryId} was not found.",
            ErrorType.NotFound
        );

    public static DomainError SKUAlreadyExists(string sku) =>
        new(
            "Product.SKUAlreadyExists",
            $"Product with SKU {sku} already exists.",
            ErrorType.Conflict
        );

    public static DomainError NotFound(Guid id) =>
        new("Product.NotFound", $"Product {id} was not found.", ErrorType.NotFound);

    public static DomainError ImageNotFound(Guid id) =>
        new("Product.ImageNotFound", $"Image {id} was not found.", ErrorType.NotFound);

    public static DomainError ProductImagesNotFound(Guid id) =>
        new(
            "Product.ProductImagesNotFound",
            $"Product images for product {id} were not found.",
            ErrorType.NotFound
        );

    public static readonly DomainError ImageBlobNameEmpty = new(
        "Product.ImageBlobNameEmpty",
        "Blob file name cannot be empty",
        ErrorType.Validation
    );

    public static readonly DomainError ImageContentTypeEmpty = new(
        "Product.ImageContentTypeEmpty",
        "Image content type cannot be empty",
        ErrorType.Validation
    );

    public static readonly DomainError MaxImagesLimitReached = new(
        "Product.MaxImagesLimitReached",
        "Maximum limit of product images has been reached",
        ErrorType.Failure
    );
}
