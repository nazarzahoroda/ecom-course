using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcomCourse.Application.Behaviors;

public partial class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
#pragma warning disable CA2016 
            return await next();
#pragma warning restore CA2016
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMilliseconds)
            {
                var requestName = typeof(TRequest).Name;

                LogLongRunningRequest(_logger, requestName, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request: {RequestName} ({ElapsedMilliseconds} milliseconds)")]
    private static partial void LogLongRunningRequest(ILogger logger, string requestName, long elapsedMilliseconds);
}