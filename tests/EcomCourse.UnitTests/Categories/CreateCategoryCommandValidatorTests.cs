using EcomCourse.Application.Categories.Commands.Create;
using FluentValidation.TestHelper;

namespace EcomCourse.UnitTests.Categories;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Should_ReturnError_When_Name_Is_Empty()
    {
        var command = new CreateCategoryCommand("");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_ReturnError_When_Name_Is_Longer_Than_100_Characters()
    {
        var command = new CreateCategoryCommand(new string('N', 101));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
