using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Carts
{
    public static class CartErrors
    {
        public static readonly DomainError InvalidQuantity = new("CartItem.InvalidQuantity", "Quantity must be greater than zero");

        public static readonly DomainError NotFound = new("Cart.NotFound", "Not found");

        public static readonly DomainError CartNotActive = new("Cart.NotActive", "Cart is not active");
    }
}
