using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Orders;

public static class OrderErrors
{
    public static readonly DomainError NotFound = new(
        "Order.NotFound",
        "The order with the specified identifier was not found.");

    public static readonly DomainError EmptyLines = new(
        "Order.EmptyLines",
        "The order must contain at least one line.");

    public static readonly DomainError InvalidQuantity = new(
        "Order.InvalidQuantity",
        "Product quantity must be greater than zero.");

    public static readonly DomainError InvalidUnitPrice = new(
        "Order.InvalidUnitPrice",
        "Product unit price cannot be negative.");

    public static readonly DomainError InvalidStatusTransition = new(
    "Order.InvalidStatusTransition",
    "This order status transition is not allowed.");
}
