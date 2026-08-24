using EcomCourse.Domain.Orders;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration);

        services.AddDbContext<EcomCourseDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<EcomCourseDbContext>());

        return services;
    }
}
