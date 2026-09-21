using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Carts
{
    public static class CartErrors
    {
        public static readonly DomainError InvalidQuantity = new(
        "CartItem.InvalidQuantity",
        "Quantity must be greater than zero",
        ErrorType.Validation
        );

        public static readonly DomainError CartNotFound = new(
            "Cart.NotFound",
            "Not found",
            ErrorType.NotFound
        );

        public static readonly DomainError CartIsEmpty = new(
            "Cart.IsEmpty",
            "Cart is empty",
            ErrorType.Conflict
        );

        public static readonly DomainError CartItemNotFound = new(
            "CartItem.NotFound",
            "Not found",
            ErrorType.NotFound
        );

        public static readonly DomainError CartNotActive = new(
            "Cart.NotActive",
            "Cart is not active",
            ErrorType.Conflict
        );

        public static readonly DomainError ActiveCartAlreadyExists = new(
            "Cart.ActiveCartAlreadyExists",
            "Customer already has an active cart",
            ErrorType.Conflict
        );
    }

}
