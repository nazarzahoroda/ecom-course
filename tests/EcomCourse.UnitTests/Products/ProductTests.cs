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

        [Fact]
        public void Update_WithValidData_ShouldUpdateProduct()
        {
            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                Guid.NewGuid());

            var product = productResult.Value!;
            var newCategoryId = Guid.NewGuid();

            var result = product.Update(
                "iPhone 16",
                999.99m,
                Currency.USD,
                "IPH-1234",
                newCategoryId);

            Assert.True(result.IsSuccess);
            Assert.Equal("iPhone 16", product.Name);
            Assert.Equal(999.99m, product.Price.Amount);
            Assert.Equal(Currency.USD, product.Price.Currency);
            Assert.Equal("IPH-1234", product.SKU.Value);
            Assert.Equal(newCategoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithEmptyName_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                string.Empty,
                999.99m,
                Currency.USD,
                "IPH-1234",
                Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.ProductNameEmpty, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithNegativeAmount_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                "iPhone 16",
                -1m,
                Currency.USD,
                "IPH-1234",
                Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(PriceErrors.AmountInvalid, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithInvalidCurrency_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                "iPhone 16",
                999.99m,
                (Currency)999,
                "IPH-1234",
                Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(PriceErrors.CurrencyInvalid, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithInvalidSKU_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                "iPhone 16",
                999.99m,
                Currency.USD,
                "INVALID",
                Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(SKUErrors.SKUInvalidFormat, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithEmptyCategoryId_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                "iPhone 16",
                999.99m,
                Currency.USD,
                "IPH-1234",
                Guid.Empty);

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.CategoryIdEmpty, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }

        [Fact]
        public void Update_WithNameTooLong_ShouldReturnFailureAndKeepOriginalState()
        {
            var categoryId = Guid.NewGuid();

            var productResult = Product.Create(
                "Samsung",
                100m,
                Currency.UAH,
                "TVL-2026",
                categoryId);

            var product = productResult.Value!;

            var result = product.Update(
                new string('A', 101),
                999.99m,
                Currency.USD,
                "IPH-1234",
                Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.ProductNameTooLong, result.Error);

            Assert.Equal("Samsung", product.Name);
            Assert.Equal(100m, product.Price.Amount);
            Assert.Equal(Currency.UAH, product.Price.Currency);
            Assert.Equal("TVL-2026", product.SKU.Value);
            Assert.Equal(categoryId, product.CategoryId);
        }
    }
}
