using System.Diagnostics.Metrics;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Domain.Customers;

public class AddressTests
{


    [Fact]
    public void CreatesAddressWhenValuesAreValid()
    {
        var street = "Polubota";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Address.Create(street, city, postalCode, country);


        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value!.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(postalCode, result.Value.PostalCode);
        Assert.Equal(country, result.Value.Country);


    }


    [Fact]
    public void ReturnsFailureWhenStreetIsEmpty()
    {
        var street = "";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Address.Create(street, city, postalCode, country);



        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.StreetRequired, result.Error);
        

     }


    [Fact]
    public void ReturnsFailureWhenCityIsEmpty()
    {
        var street = "Polubotla";
        var city = "";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Address.Create(street, city, postalCode, country);



        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.CityRequired, result.Error);


    }


    [Fact]
    public void ReturnsFailureWhenPostalCodeIsEmpty()
    {
        var street = "Polubotla";
        var city = "Lviv";
        var postalCode = "";
        var country = "Ukraine";

        var result = Address.Create(street, city, postalCode, country);



        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.PostalCodeRequired, result.Error);


    }

    [Fact]
    public void ReturnsFailureWhenCountryIsEmpty()
    {
        var street = "Polubotla";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "";

        var result = Address.Create(street, city, postalCode, country);



        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.CountryRequired, result.Error);


    }


}
