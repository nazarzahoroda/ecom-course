using EcomCourse.Api.Common;
using EcomCourse.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EcomCourse.UnitTests.Api;

public class ResultExtensionsTests
{
    [Fact]
    public void ToProblemDetails_WhenValidationError_ReturnsBadRequest()
    {
        // Arrange
        var error = new DomainError(
            "Test.Validation",
            "Validation error.",
            ErrorType.Validation);

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal(error.Code, problemDetails.Title);
        Assert.Equal(error.Description, problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WhenNotFoundError_ReturnsNotFound()
    {
        // Arrange
        var error = new DomainError(
            "Test.NotFound",
            "Resource was not found.",
            ErrorType.NotFound);

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal(error.Code, problemDetails.Title);
        Assert.Equal(error.Description, problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WhenConflictError_ReturnsConflict()
    {
        // Arrange
        var error = new DomainError(
            "Test.Conflict",
            "Resource already exists.",
            ErrorType.Conflict);

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal(error.Code, problemDetails.Title);
        Assert.Equal(error.Description, problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WhenUnauthorizedError_ReturnsUnauthorized()
    {
        // Arrange
        var error = new DomainError(
            "Test.Unauthorized",
            "Authentication is required.",
            ErrorType.Unauthorized);

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
        Assert.Equal(error.Code, problemDetails.Title);
        Assert.Equal(error.Description, problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WhenResultIsSuccessful_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Success();

        // Act
        Action action = () => result.ToProblemDetails();

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void ToProblemDetails_WhenErrorTypeIsUnsupported_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var error = new DomainError(
            "Test.Unsupported",
            "Unsupported error.",
            ErrorType.None);

        var result = Result.Failure(error);

        // Act
        Action action = () => result.ToProblemDetails();

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
}
