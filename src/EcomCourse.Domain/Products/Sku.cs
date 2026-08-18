using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Products
{
    public sealed class Sku : ValueObject
    {
        private static readonly Regex _skuRegex = new("^[A-Z]{3}-\\d{4}$", RegexOptions.Compiled);

        public string Value { get; }

        private Sku(string value)
        {
            Value = value;
        }

        public static Result<Sku> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<Sku>(ProductErrors.EmptySku);
            }

            var trimmed = value.Trim();

            if (!_skuRegex.IsMatch(trimmed))
            {
                return Result.Failure<Sku>(ProductErrors.InvalidSkuFormat);
            }

            return Result.Success(new Sku(trimmed));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
