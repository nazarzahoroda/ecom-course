using EcomCourse.Domain.Carts;

namespace EcomCourse.UnitTests
{
    public class CartTests
    {
        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActive()
        {
            var cart = new Cart(
                Guid.NewGuid(), Guid.NewGuid());

            cart.Abandon();

            var result = cart.AddItem(Guid.NewGuid(), 1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
        }
        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActiveCheckout()
        {
            var cart = new Cart(
                Guid.NewGuid(), Guid.NewGuid());

            cart.Checkout();

            var result = cart.AddItem(Guid.NewGuid(), 1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
        }
        [Fact]
        public void CreateItemQuantityWithZeroQuantityHaveToFail()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var productId = Guid.NewGuid();

            var result = cart.AddItem(productId, 0);


            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }
        [Fact]
        public void UpdateItemQuantityWithZeroQuantityHaveToFail()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);

            var result = cart.AddItem(productId, 0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }
    }
}
