using EcomCourse.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Behaviors;

public class ValidationBehaviorTests
{
    public record TestCommand : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestCommand, string>(Enumerable.Empty<IValidator<TestCommand>>());
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("Success");

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validatorMock = Substitute.For<IValidator<TestCommand>>();
        var failures = new List<ValidationFailure> { new("Property", "Error message") };

        validatorMock
            .ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestCommand, string>(new[] { validatorMock });
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestCommand(), next, CancellationToken.None));

        await next.DidNotReceive().Invoke();
    }
}