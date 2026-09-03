using EcomCourse.Application.Products.Commands.Update;
using EcomCourse.Domain.Products;
using FluentValidation.TestHelper;

namespace EcomCourse.UnitTests.Products;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            Id = Guid.Empty
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            Name = string.Empty
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_WhenNameIsTooLong_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            Name = new string('A', 101)
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_WhenAmountIsNegative_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            Amount = -1m
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task Validate_WhenCurrencyIsInvalid_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            Currency = (Currency)999
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public async Task Validate_WhenSkuIsEmpty_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            SKU = string.Empty
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.SKU);
    }

    [Fact]
    public async Task Validate_WhenSkuHasInvalidFormat_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            SKU = "INVALID"
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.SKU);
    }

    [Fact]
    public async Task Validate_WhenCategoryIdIsEmpty_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with
        {
            CategoryId = Guid.Empty
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static UpdateProductCommand CreateValidCommand()
    {
        return new UpdateProductCommand(
            Guid.NewGuid(),
            "iPhone 16",
            999.99m,
            Currency.USD,
            "IPH-1234",
            Guid.NewGuid());
    }
}
