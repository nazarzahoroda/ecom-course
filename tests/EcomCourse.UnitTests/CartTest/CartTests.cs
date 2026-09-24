using EcomCourse.Domain.Carts;

namespace EcomCourse.UnitTests.CartTest
{
    public class CartTests
    {
        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActive()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            cart.Abandon();

            var result = cart.AddItem(Guid.NewGuid(), 1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
        }

        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActiveCheckout()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

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

            var result = cart.UpdateItemQuantity(productId, 0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }

        [Fact]
        public void UpdateItemQuantity_WithValidQuantity_ShouldUpdateQuantity()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);

            var result = cart.UpdateItemQuantity(productId, 5);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, cart.Items.Single().Quantity);
        }

        [Fact]
        public void UpdateItemQuantity_WithNegativeQuantity_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);

            var result = cart.UpdateItemQuantity(productId, -1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
            Assert.Equal(2, cart.Items.Single().Quantity);
        }

        [Fact]
        public void UpdateItemQuantity_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var result = cart.UpdateItemQuantity(Guid.NewGuid(), 2);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartItemNotFound, result.Error);
        }

        [Fact]
        public void AddItem_WhenProductAlreadyInCart_SumsQuantity()
        {
            // Arrange
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            // Act
            cart.AddItem(productId, 2);
            var result = cart.AddItem(productId, 3);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(cart.Items);
            Assert.Equal(5, cart.Items.First().Quantity);
        }

        [Fact]
        public void RemoveItem_WhenProductExists_ShouldRemoveItem()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);

            var result = cart.RemoveItem(productId);

            Assert.True(result.IsSuccess);
            Assert.Empty(cart.Items);
        }

        [Fact]
        public void RemoveItem_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var result = cart.RemoveItem(Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartItemNotFound, result.Error);
        }

        [Fact]
        public void RemoveItem_WhenCartIsNotActive_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);
            cart.Checkout();

            var result = cart.RemoveItem(productId);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
            Assert.Single(cart.Items);
        }

        [Fact]
        public void Checkout_WhenCartIsActive_ShouldChangeStatusToCheckedOut()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var result = cart.Checkout();

            Assert.True(result.IsSuccess);
            Assert.Equal(CartStatus.CheckedOut, cart.Status);
        }

        [Fact]
        public void Checkout_WhenCartIsNotActive_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            cart.Abandon();

            var result = cart.Checkout();

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
            Assert.Equal(CartStatus.Abandoned, cart.Status);
        }

        [Fact]
        public void Abandon_WhenCartIsActive_ShouldChangeStatusToAbandoned()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            var result = cart.Abandon();

            Assert.True(result.IsSuccess);
            Assert.Equal(CartStatus.Abandoned, cart.Status);
        }

        [Fact]
        public void Abandon_WhenCartIsNotActive_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());

            cart.Checkout();

            var result = cart.Abandon();

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
            Assert.Equal(CartStatus.CheckedOut, cart.Status);
        }

        [Fact]
        public void AddItem_WithValidProduct_ShouldAddItemToCart()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            var result = cart.AddItem(productId, 2);

            Assert.True(result.IsSuccess);

            var item = Assert.Single(cart.Items);
            Assert.Equal(productId, item.ProductId);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void AddItem_WhenQuantityIsNegative_ShouldReturnFailure()
        {
            var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
            var productId = Guid.NewGuid();

            var result = cart.AddItem(productId, -1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
            Assert.Empty(cart.Items);
        }
    }
}
