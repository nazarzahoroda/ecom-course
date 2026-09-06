using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Domain.Customers;

public class EmailTests
{

    [Fact]
    public void CreatesEmailWhenValueIsValid()
    {

        var value = "test@example.com";

        var result = Email.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value!.Value);
    }



    [Fact]
    public void ReturnsFailureWhenValueIsEmpty()
    {

        var value = "";

        var result = Email.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.EmailRequired, result.Error);

    }


    [Fact]
    public void ReturnsFailureWhenValueHasInvalidFormat()
    {
        var value = "not-an-email";

        var result = Email.Create(value);


        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.EmailInvalidFormat, result.Error);
    }


    [Fact]

    public void NormalizesValueWhenEmailIsValid()
    {
        var value = " TEST@EXAMPLE.COM ";

        var result = Email.Create(value);


        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value!.Value);
    }

}
