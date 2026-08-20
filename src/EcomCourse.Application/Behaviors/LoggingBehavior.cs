using MediatR;
using Microsoft.Extensions.Logging;

namespace EcomCourse.Application.Behaviors;

public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        LogProcessingRequest(_logger, requestName);

        try
        {
#pragma warning disable CA2016 
            return await next();
#pragma warning restore CA2016
        }
        finally
        {
            LogCompletedRequest(_logger, requestName);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing request {RequestName}")]
    private static partial void LogProcessingRequest(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed request {RequestName}")]
    private static partial void LogCompletedRequest(ILogger logger, string requestName);
}