using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Domain.Customers;

public class CustomerTests
{

    [Fact]
    public void CreatesCustomerWhenValuesAreValid()
    {
        var name = "Ivan";
        var userId = Guid.NewGuid();
        var email = "ivan@example.com";
        var street = "Polubotka";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Customer.Create(userId, name, email, street, city, postalCode, country);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(email, result.Value.Email.Value);
        Assert.Equal(street, result.Value.Address.Street);
        Assert.Equal(city, result.Value.Address.City);
        Assert.Equal(postalCode, result.Value.Address.PostalCode);
        Assert.Equal(country, result.Value.Address.Country);
    }

    [Fact]
    public void ReturnsFailureWhenNameIsEmpty()
    {
        var name = "";
        var userId = Guid.NewGuid();
        var email = "ivan@example.com";
        var street = "Polubotka";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Customer.Create(userId, name, email, street, city, postalCode, country);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NameRequired, result.Error);
    }

    [Fact]
    public void ReturnsFailureWhenEmailIsInvalid()
    {
        var name = "Ivan";
        var userId = Guid.NewGuid();
        var email = "invalid-email";
        var street = "Polubotka";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Customer.Create(userId, name, email, street, city, postalCode, country);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.EmailInvalidFormat, result.Error);
    }

    [Fact]
    public void ReturnsFailureWhenAddressIsInvalid()
    {
        var name = "Ivan";
        var userId = Guid.NewGuid();
        var email = "ivan@example.com";
        var street = "";
        var city = "Lviv";
        var postalCode = "79066";
        var country = "Ukraine";

        var result = Customer.Create(userId, name, email, street, city, postalCode, country);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.StreetRequired, result.Error);
    }
}


