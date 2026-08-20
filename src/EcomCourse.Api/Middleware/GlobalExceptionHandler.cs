using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EcomCourse.Api.Middleware;

public sealed partial class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unhandled exception while processing request")]
    private partial void LogUnhandledException(Exception exception);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var validationProblemDetails = new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray()))
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
            return true;
        }

        LogUnhandledException(exception);

        var problemDetails = new ProblemDetails
        {
            Title = "Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}