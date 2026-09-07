using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests.Products
{
    public class PriceTests
    {
        [Fact]
        public void Create_WithInvalidPriceAmount_ShouldReturnFailure()
        {

            var amount = -100m;
            var currency = Currency.UAH;

            var resultPrice = Price.Create(amount, currency);

            Assert.True(resultPrice.IsFailure);
            Assert.Equal(PriceErrors.AmountInvalid, resultPrice.Error);
        }

        [Fact]
        public void Create_WithInvalidCurrency_ShouldReturnFailure()
        {
            var amount = 100m;
            var currency = (Currency)999;

            var resultPrice = Price.Create(amount, currency);

            Assert.True(resultPrice.IsFailure);
            Assert.Equal(PriceErrors.CurrencyInvalid, resultPrice.Error);
        }
    }
}
