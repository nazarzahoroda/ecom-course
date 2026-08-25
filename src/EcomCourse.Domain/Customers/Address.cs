using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Customers;

public sealed class Address : ValueObject
{


    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }


    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    private Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }



    public static Result<Address> Create(
    string street,
    string city,
    string postalCode,
    string country)
    {


        if (string.IsNullOrWhiteSpace(street))
        {
            return Result.Failure<Address>(CustomerErrors.StreetRequired);
        }


        if (string.IsNullOrWhiteSpace(city))
        {
            return Result.Failure<Address>(CustomerErrors.CityRequired);
        }


        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return Result.Failure<Address>(CustomerErrors.PostalCodeRequired);
        }


        if (string.IsNullOrWhiteSpace(country)
)
        {
            return Result.Failure<Address>(CustomerErrors.CountryRequired);
        }



        return Result.Success(new Address(
    street.Trim(),
    city.Trim(),
    postalCode.Trim(),
    country.Trim()));

    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }


}
