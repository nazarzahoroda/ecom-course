using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Orders;

public static class OrderErrors
{
    public static readonly DomainError NotFound = new(
        "Order.NotFound",
        "Замовлення з вказаним ідентифікатором не знайдено.");

    public static readonly DomainError EmptyLines = new(
        "Order.EmptyLines",
        "Замовлення повинно містити хоча б одну лінію.");

    public static readonly DomainError InvalidQuantity = new(
        "Order.InvalidQuantity",
        "Кількість товару має бути більшою за нуль.");

    public static readonly DomainError InvalidUnitPrice = new(
        "Order.InvalidUnitPrice",
        "Ціна товару не може бути від'ємною.");
}