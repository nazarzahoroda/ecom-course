using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public static class SKUErrors
{
    public static readonly DomainError SKUValueEmpty = new(
        "SKU.ValueEmpty",
        "SKU value cannot be empty.",
        ErrorType.Validation);

    public static readonly DomainError SKUInvalidFormat = new(
        "SKU.InvalidFormat",
        "SKU value is in an invalid format.",
        ErrorType.Validation);
}
