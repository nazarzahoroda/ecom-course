using System.Reflection;
using EcomCourse.Application.Common.Behavior;
using EcomCourse.Application.Services;
using EcomCourse.Application.Behaviors;
using EcomCourse.Application.Services;
using System.Reflection;
using EcomCourse.Application.Common.Behavior;
using EcomCourse.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSingleton(TimeProvider.System);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });
        services.AddScoped<CompensateAsync>();
        return services;
    }
}
