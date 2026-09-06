using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Products;

public sealed class Price
{
    private Price(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public Currency Currency { get; private set; }

    public static Result<Price> Create(decimal amount, Currency currency)  
    {
        if (amount < 0)
        {
            return Result.Failure<Price>(PriceErrors.AmountInvalid);      
        }

        return Result.Success(new Price(amount, currency));
    }
}
