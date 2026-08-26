using System.Text.Json;
using EcomCourse.Api.Middleware;
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
    public async Task TryHandleAsync_GenericExceptionInDevelopment_Returns500WithStackTrace()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        Exception exception;
        try
        {
            throw new InvalidOperationException("Something exploded");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _context.Response.StatusCode);

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();

        Assert.Contains("detail", responseBody);
        Assert.Contains("GlobalExceptionHandlerTests", responseBody);
    }

    [Fact]
    public async Task TryHandleAsync_GenericExceptionInProduction_Returns500WithoutStackTrace()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        Exception exception;
        try
        {
            throw new InvalidOperationException("Something exploded");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var result = await _handler.TryHandleAsync(_context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _context.Response.StatusCode);

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();

        Assert.Contains("An unexpected error occurred", responseBody);
        Assert.DoesNotContain("GlobalExceptionHandlerTests", responseBody);
    }
}