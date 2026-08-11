using System.Reflection.Metadata;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Application
{
    internal class DependencyInjection
    {
        public static void AddApplication(IServiceCollection services)
        {
            var assembly = typeof(AssemblyReference).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
