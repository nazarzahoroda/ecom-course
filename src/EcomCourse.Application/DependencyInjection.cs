using EcomCourse.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        services.AddScoped<CompensateAsync>();
        return services;
    }
}
