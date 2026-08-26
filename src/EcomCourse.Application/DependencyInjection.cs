using EcomCourse.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        return services;
    }
}