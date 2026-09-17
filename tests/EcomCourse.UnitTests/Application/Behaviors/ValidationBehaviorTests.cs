using EcomCourse.Application.Common.Behavior;
using EcomCourse.Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace EcomCourse.UnitTests.Application.Behaviors
{
    public class ValidationBehaviorTests
    {
        public record TestCommand(string Name, int Quantity) : IRequest<Result>;

        [Fact]
        public async Task Handle_Should_NotInvokeNext_WhenValidationFails()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<TestCommand>>();
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Quantity", "Quantity must be greater than 0."),
                new ValidationFailure("Name", "Name is required.")
            };

            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

            var validators = new List<IValidator<TestCommand>> { validatorMock.Object };
            var behavior = new ValidationBehavior<TestCommand, Result>(validators);

            var command = new TestCommand("", 0);
            var nextDelegateMock = new Mock<RequestHandlerDelegate<Result>>();

            // Act
            var result = await behavior.Handle(command, nextDelegateMock.Object, CancellationToken.None);

            // Assert
            nextDelegateMock.Verify(x => x(), Times.Never);
            Assert.True(result.IsFailure);

            var validationResult = Assert.IsAssignableFrom<IValidationResult>(result);
            Assert.Equal(2, validationResult.Errors.Length);
            Assert.Contains(validationResult.Errors, e => e.Code == "Quantity" && e.Description == "Quantity must be greater than 0.");
            Assert.Contains(validationResult.Errors, e => e.Code == "Name" && e.Description == "Name is required.");
        }

        [Fact]
        public async Task Handle_Should_InvokeNext_WhenValidationSucceeds()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<TestCommand>>();
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var validators = new List<IValidator<TestCommand>> { validatorMock.Object };
            var behavior = new ValidationBehavior<TestCommand, Result>(validators);

            var command = new TestCommand("Valid Product", 5);
            var nextDelegateMock = new Mock<RequestHandlerDelegate<Result>>();
            nextDelegateMock.Setup(x => x()).ReturnsAsync(Result.Success());

            // Act
            var result = await behavior.Handle(command, nextDelegateMock.Object, CancellationToken.None);

            // Assert
            nextDelegateMock.Verify(x => x(), Times.Once);
            Assert.True(result.IsSuccess);
        }
    }
}