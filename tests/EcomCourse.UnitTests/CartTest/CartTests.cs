using System;
using EcomCourse.Domain.Carts;
using Xunit;

namespace EcomCourse.UnitTests.CartTest
{
    public class CartTests
    {
        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActive()
        {
            var cart = Cart.Create(Guid.NewGuid()).Value!;

            cart.Abandon();

            var result = cart.AddItem(Guid.NewGuid(), 1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
        }

        [Fact]
        public void ItemCannotBeAddedToCartWhenNotActiveCheckout()
        {
            var cart = Cart.Create(Guid.NewGuid()).Value!;

            cart.Checkout();

            var result = cart.AddItem(Guid.NewGuid(), 1);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.CartNotActive, result.Error);
        }

        [Fact]
        public void CreateItemQuantityWithZeroQuantityHaveToFail()
        {
            var cart = Cart.Create(Guid.NewGuid()).Value!;

            var productId = Guid.NewGuid();

            var result = cart.AddItem(productId, 0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }

        [Fact]
        public void UpdateItemQuantityWithZeroQuantityHaveToFail()
        {
            var cart = Cart.Create(Guid.NewGuid()).Value!;

            var productId = Guid.NewGuid();

            cart.AddItem(productId, 2);

            var result = cart.UpdateItemQuantity(productId, 0);

            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.InvalidQuantity, result.Error);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenCustomerIdIsEmpty()
        {
            // Act
            var result = Cart.Create(Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(CartErrors.EmptyCustomerId, result.Error);
        }
    }

}

