using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public static class PriceErrors
{
    public static readonly DomainError AmountInvalid = new(
        "Price.AmountInvalid",
        "Price cannot be negative.");
}
