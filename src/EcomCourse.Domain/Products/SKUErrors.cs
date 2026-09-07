using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public static class SKUErrors
{
    public static readonly DomainError SKUValueEmpty = new DomainError("SKU.ValueEmpty",
        "SKU value cannot be empty.");

    public static readonly DomainError SKUInvalidFormat = new DomainError("SKU.InvalidFormat",
        "SKU value is in an invalid format.");
}
