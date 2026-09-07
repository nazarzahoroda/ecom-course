using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests.Products
{
    public class SKUTests
    {
        [Fact]
        public void Create_WithInvalidSKUFormat_ShouldReturnFailure()
        {
            var sku = "TVl-202";

            var resultSKU = SKU.Create(sku);

            Assert.True(resultSKU.IsFailure);
            Assert.Equal(SKUErrors.SKUInvalidFormat, resultSKU.Error);
        }

        [Fact]
        public void Create_WithInvalidSKUValueEmpty_ShouldReturnFailure()
        {
            var sku = "";

            var resultSKU = SKU.Create(sku);

            Assert.True(resultSKU.IsFailure);
            Assert.Equal(SKUErrors.SKUValueEmpty, resultSKU.Error);
        }

        [Fact]
        public void Create_WithValidSKU_ShouldReturnSuccess()
        {
            var sku = "TVL-2026";

            var resultSKU = SKU.Create(sku);

            Assert.True(resultSKU.IsSuccess);
            Assert.Equal(sku, resultSKU.Value!.Value);
        }
    }
}
