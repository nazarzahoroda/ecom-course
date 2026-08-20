using Application.Categories;
using FluentValidation.TestHelper;

namespace Tests;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator validator = new();

    [Fact]
    public void Should_ReturnError_When_Name_Is_Empty()
    {
        var command = new CreateCategoryCommand("");

        var result = validator.TestValidate(command);

        result.ShouldReturnValidationError(x => x.Name);
    }

    [Fact]
    public void Should_ReturnError_When_Name_Is_Longer_Than_100_Characters()
    {
        var command = new CreateCategoryCommand(new string('N', 101));
            
        var result = validator.TestValidate(command);

        result.ShouldReturnValidationError(x => x.Name);
    }
}
