<<<<<<< ours
using EcomCourse.Application.Behaviors;
=======
using EcomCourse.Application.Services;
>>>>>>> theirs
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
<<<<<<< ours
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSingleton(TimeProvider.System);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

=======
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        services.AddScoped<CompensateAsync>();
>>>>>>> theirs
        return services;
    }
}
