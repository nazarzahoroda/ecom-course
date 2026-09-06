
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products
{
    public static class ProductErrors
    {
        public static readonly DomainError ProductNameEmpty = new(
            "ProductNameEmpty",
            "You need enter name Product.");

        public static readonly DomainError ProductNameTooLong = new(
            "ProductNameTooLong",
            "Product name must be contain less 101 characters.");

        public static readonly DomainError CategoryIdEmpty = new(
            "CategoryIdEmpty",
            "Category Id cannot be empty.");
    }
}
