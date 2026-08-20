using System.Text.Json;
using EcomCourse.Api.Middleware;
using EcomCourse.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EcomCourse.UnitTests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _context;

    public GlobalExceptionHandlerTests()
    {
        _envMock = new Mock<IHostEnvironment>();
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        _handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            _envMock.Object);

        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
        _context.TraceIdentifier = "test-trace-id";
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400BadRequest()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Property1", "Error 1") };
        var exception = new ValidationException(failures);

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, _context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404NotFound()
    {
        // Arrange
        var exception = new NotFoundException("User not found");

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status404NotFound, _context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Returns409Conflict()
    {
        // Arrange
        var exception = new ConflictException("Email already exists");

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status409Conflict, _context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_GenericExceptionInDevelopment_Returns500WithStackTrace()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var exception = new InvalidOperationException("Something exploded");

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _context.Response.StatusCode);

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
        Assert.Contains("stackTrace", responseBody);
    }

    [Fact]
    public async Task TryHandleAsync_GenericExceptionInProduction_Returns500WithoutStackTrace()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var exception = new InvalidOperationException("Something exploded");

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _context.Response.StatusCode);

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("stackTrace", responseBody);
    }
}