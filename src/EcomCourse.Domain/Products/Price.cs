using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Products
{
    public sealed class Price : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Price(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Result<Price> Create(decimal amount, string currency = "USD")
        {
            if (amount < 0)
            {
                return Result.Failure<Price>(ProductErrors.NegativePrice);
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return Result.Failure<Price>(ProductErrors.EmptyCurrency);
            }

            return Result.Success(new Price(amount, currency.Trim().ToUpperInvariant()));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
