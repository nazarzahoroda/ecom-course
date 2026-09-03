using EcomCourse.Application.Products.Commands.Delete;

namespace EcomCourse.UnitTests.Products;

public class DeleteProductCommandValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithEmptyId_ShouldReturnFailure()
    {
        var command = new DeleteProductCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithValidId_ShouldReturnSuccess()
    {
        var command = new DeleteProductCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
