using System.Text.RegularExpressions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products
{
    public sealed class SKU
    {
        public string Value { get; private set; }

        private SKU(string value)
        {
            Value = value;
        }

        public static Result<SKU> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<SKU>(SKUErrors.SKUValueEmpty);
            }

            if (!Regex.IsMatch(value, @"^[A-Z]{3}-\d{4}$"))
            {
                return Result.Failure<SKU>(SKUErrors.SKUInvalidFormat);
            }

            return Result.Success(new SKU(value));
        }
    }
}
