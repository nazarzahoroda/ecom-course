using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests.Products
{
    public class ProductTests
    {
        [Fact]
        public void Create_WithValidData_ShouldReturnSuccess()
        {
            var name = "Samsung";
            var amount = 100m;
            var currency = Currency.UAH;
            var sku = "TVL-2026";
            var categoryId = Guid.NewGuid();


            var result = Product.Create(name, amount, currency, sku, categoryId);


            Assert.True(result.IsSuccess);
            Assert.Equal(name, result.Value!.Name);
            Assert.Equal(categoryId, result.Value.CategoryId);
            Assert.Equal(amount, result.Value.Price.Amount);
            Assert.Equal(currency, result.Value.Price.Currency);
            Assert.Equal(sku, result.Value.SKU.Value);
        }

        [Fact]
        public void Create_WithInvalidName_ShouldReturnFailure()
        {
            var name = "";
            var amount = 100m;
            var currency = Currency.UAH;
            var sku = "TVL-2026";
            var categoryId = Guid.NewGuid();

            var result = Product.Create(name, amount, currency, sku, categoryId);

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.ProductNameEmpty, result.Error);
        }

        [Fact]
        public void Create_WithValidData_NameTooLong()
        {
            var name = new string('A', 101);
            var amount = 100m;
            var currency = Currency.UAH;
            var sku = "TVL-2026";
            var categoryId = Guid.NewGuid();

            var result = Product.Create(name, amount, currency, sku, categoryId);

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.ProductNameTooLong, result.Error);
        }

        [Fact]
        public void Create_WithInvalidAmount_ShouldReturnFailure()
        {
            var name = "Samsung";
            var amount = -100m;
            var currency = Currency.UAH;
            var sku = "TVL-2026";
            var categoryId = Guid.NewGuid();

            var result = Product.Create(name, amount, currency, sku, categoryId);

            Assert.True(result.IsFailure);
            Assert.Equal(PriceErrors.AmountInvalid, result.Error);
        }

        [Fact]
        public void Create_WithInvalidSKUFormat_ShouldReturnFailure()
        {
            var name = "Samsung";
            var amount = 100m;
            var currency = Currency.UAH;
            var sku = "TVl-202";
            var categoryId = Guid.NewGuid();

            var result = Product.Create(name, amount, currency, sku, categoryId);

            Assert.True(result.IsFailure);
            Assert.Equal(SKUErrors.SKUInvalidFormat, result.Error);
        }

        [Fact]
        public void Create_WithCategoryIdEmpty_ShouldReturnFailure()
        {
            var name = "Samsung";
            var amount = 100m;
            var currency = Currency.UAH;
            var sku = "TVL-2026";
            var categoryId = Guid.Empty;

            var result = Product.Create(name, amount, currency, sku, categoryId);

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.CategoryIdEmpty, result.Error);
        }

        
    }
}
