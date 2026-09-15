using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            LogUnhandledException(_logger, exception.Message, exception);

            var problemDetails = new ProblemDetails
            {
                Title = "Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred",
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "An unhandled exception occurred: {ErrorMessage}")]
        private static partial void LogUnhandledException(ILogger logger, string errorMessage, Exception exception);
    }
}
