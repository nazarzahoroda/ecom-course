using EcomCourse.Domain.Carts;

namespace EcomCourse.UnitTests.CartTest
{
    public class CartItemTests
    {
        [Fact]
        public void Create_WithValidQuantity_ShouldReturnSuccess()
        {
            var productId = Guid.NewGuid();

            var result = CartItem.Create(productId, 2);

            Assert.True(result.IsSuccess);
            Assert.Equal(productId, result.Value!.ProductId);
            Assert.Equal(2, result.Value.Quantity);
        }

        [Fact]
        public void Create_WithInvalidQuantity_ShouldReturnFailure()
        {
            var result = CartItem.Create(Guid.NewGuid(), 0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }

        [Fact]
        public void ChangeQuantity_WithValidQuantity_ShouldUpdateQuantity()
        {
            var item = CartItem.Create(Guid.NewGuid(), 2).Value!;

            var result = item.ChangeQuantity(5);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, item.Quantity);
        }

        [Fact]
        public void ChangeQuantity_WithInvalidQuantity_ShouldReturnFailureAndPreserveQuantity()
        {
            var item = CartItem.Create(Guid.NewGuid(), 2).Value!;

            var result = item.ChangeQuantity(0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
            Assert.Equal(2, item.Quantity);
        }
    }
}
