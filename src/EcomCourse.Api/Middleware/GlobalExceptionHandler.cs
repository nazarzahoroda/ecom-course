using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Middleware
{
    public partial class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            ProblemDetails problemDetails;

            if (exception is UnauthorizedAccessException unauthorizedException)
            {
                LogUnauthorizedException(
                    _logger,
                    unauthorizedException,
                    httpContext.Request.Method,
                    httpContext.Request.Path
                );

                problemDetails = new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = unauthorizedException.Message,
                };
            }
            else
            {
                LogUnhandledException(
                    _logger,
                    exception,
                    httpContext.Request.Method,
                    httpContext.Request.Path
                );

            var isUnauthorized = exception is UnauthorizedAccessException;
            var statusCode = isUnauthorized
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Title = isUnauthorized ? "Unauthorized" : "Server Error",
                Status = statusCode,
                Detail = _env.IsDevelopment()
                    ? isUnauthorized ? exception.Message : exception.ToString()
                    : isUnauthorized ? "Authentication is required." : "An unexpected error occurred.",
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = "Unauthorized access attempt while processing {Method} {Path}"
        )]
        private static partial void LogUnauthorizedException(
            ILogger logger,
            Exception exception,
            string method,
            string path
        );

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Unhandled exception while processing {Method} {Path}"
        )]
        private static partial void LogUnhandledException(
            ILogger logger,
            Exception exception,
            string method,
            string path
        );
    }
}
