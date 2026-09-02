using EcomCourse.Application.Products.Commands.Create;
using EcomCourse.Domain.Products;
using FluentValidation.TestHelper;

namespace EcomCourse.UnitTests.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_ReturnError_When_Name_Is_Empty()
    {
        var command = new CreateProductCommand(
            "",
            100m,
            Currency.USD,
            "ABC-1234",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_ReturnError_When_Name_Is_Longer_Than_100_Characters()
    {
        var command = new CreateProductCommand(
            new string('N', 101),
            100m,
            Currency.USD,
            "ABC-1234",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_ReturnError_When_Amount_Is_Negative()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            -1m,
            Currency.USD,
            "ABC-1234",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_ReturnError_When_SKU_Is_Empty()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            100m,
            Currency.USD,
            "",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SKU);
    }

    [Fact]
    public void Should_ReturnError_When_SKU_Has_Invalid_Format()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            100m,
            Currency.USD,
            "BAD",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SKU);
    }

    [Fact]
    public void Should_ReturnError_When_CategoryId_Is_Empty()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            100m,
            Currency.USD,
            "ABC-1234",
            Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Should_ReturnError_When_Currency_Is_Invalid()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            100m,
            (Currency)999,
            "ABC-1234",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Not_ReturnErrors_When_Command_Is_Valid()
    {
        var command = new CreateProductCommand(
            "iPhone 16",
            999.99m,
            Currency.USD,
            "IPH-1234",
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
