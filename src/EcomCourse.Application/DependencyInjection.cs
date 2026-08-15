using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EcomCourse.Application.Common.Behavior;
using FluentValidation;
using MediatR;

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

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }

    }
}
