
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products
{
    public static class ProductErrors
    {
        public static readonly DomainError ProductNameEmpty = new(
            "ProductNameEmpty",
            "You need enter name Product.");

        public static readonly DomainError ProductNameTooLong = new(
            "ProductNameTooLong",
            "Product name must be contain less 101 characters.");

        public static readonly DomainError CategoryIdEmpty = new(
            "CategoryIdEmpty",
            "Category Id cannot be empty.");

        public static DomainError CategoryNotFound(Guid categoryId) => new(
            "Product.CategoryNotFound",
            $"Category {categoryId} was not found.");

        public static DomainError SKUAlreadyExists(string sku) => new(
            "Product.SKUAlreadyExists",
            $"Product with SKU {sku} already exists.");

        public static DomainError NotFound(Guid id) => new(
            "Product.NotFound",
            $"Product {id} was not found.");
    }
}
