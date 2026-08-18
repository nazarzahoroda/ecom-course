using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EcomCourse.Domain.Products;

namespace EcomCourse.UnitTests.Domain
{
    public class ProductValueObjectsTests
    {
        [Theory]
        [InlineData(-0.01)]
        [InlineData(-100)]
        public void PriceCreateShouldReturnFailureWhenAmountIsNegative(decimal invalidAmount)
        {
            // Act
            var result = Price.Create(invalidAmount, "USD");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.NegativePrice, result.Error);
        }

        [Theory]
        [InlineData("10.50", "USD")]
        [InlineData("0", "EUR")]
        public void PriceCreateShouldReturnSuccessWhenAmountIsNonNegative(string amountStr, string currency)
        {
            // Arrange
            decimal amount = decimal.Parse(amountStr, CultureInfo.InvariantCulture);

            // Act
            var result = Price.Create(amount, currency);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(amount, result.Value.Amount);
            Assert.Equal(currency, result.Value.Currency);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("AB-1234")]       // 2 літери замість 3
        [InlineData("ABCD-1234")]     // 4 літери замість 3
        [InlineData("ABC-123")]       // 3 цифри замість 4
        [InlineData("abc-1234")]      // Малі літери
        [InlineData("ABC_1234")]      // Неправильний роздільник
        public void SkuCreateShouldReturnFailureWhenFormatIsInvalid(string invalidSku)
        {
            // Act
            var result = Sku.Create(invalidSku);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Theory]
        [InlineData("ABC-1234")]
        [InlineData("PRD-9999")]
        public void SkuCreateShouldReturnSuccessWhenFormatIsValid(string validSku)
        {
            // Act
            var result = Sku.Create(validSku);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(validSku, result.Value.Value);
        }
    }
}
