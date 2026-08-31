using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products
{
    public sealed class Product
    {
        private Product()
        {

        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = null!;

        public Price Price { get; private set; } = null!;

        public SKU SKU { get; private set; } = null!;

        public Guid CategoryId { get; private set; }


        private Product(Guid id, string name, Price price, SKU sku, Guid categoryId)
        {
            Id = id;
            Name = name;
            Price = price;
            SKU = sku;
            CategoryId = categoryId;
        }

        public static Result<Product> Create(string name, decimal amount, Currency currency, string sku, Guid categoryId)
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

            var product = new Product(Guid.NewGuid(), name.Trim(), priceResult.Value!, skuResult.Value!, categoryId);

            return Result.Success(product);
        }
    }
}
