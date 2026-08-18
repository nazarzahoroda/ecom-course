using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products
{
    public class ProductErrors
    {
        public static readonly DomainError NegativePrice = new(
            "Price.Negative",
            "Price amount cannot be negative.");

        public static readonly DomainError EmptyCurrency = new(
            "Price.EmptyCurrency",
            "Currency cannot be empty.");

        public static readonly DomainError EmptySku = new(
            "Sku.Empty",
            "SKU cannot be empty.");

        public static readonly DomainError InvalidSkuFormat = new(
            "Sku.InvalidFormat",
            "SKU must follow the format '^[A-Z]{3}-\\d{4}$' (e.g. 'ABC-1234').");

        public static readonly DomainError EmptyName = new(
            "Product.EmptyName",
            "Product name cannot be empty.");
    }
}
