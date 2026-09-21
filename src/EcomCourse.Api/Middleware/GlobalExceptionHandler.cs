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
            LogUnhandledException(
                _logger,
                exception,
                httpContext.Request.Method,
                httpContext.Request.Path
            );

            var problemDetails = new ProblemDetails
            {
                Title = "Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _env.IsDevelopment()
                    ? exception.ToString()
                    : "An unexpected error occurred.",
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

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
