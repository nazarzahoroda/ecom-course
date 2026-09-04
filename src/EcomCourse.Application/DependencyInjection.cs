using System.Reflection;
using EcomCourse.Application.Common.Behavior;
using EcomCourse.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddScoped<CompensateAsync>();
            return services;
        }
    }
}
