using EcomCourse.Application.Customers;
using EcomCourse.Infrastructure.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration);

        services.Configure<DatabaseConnectionOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
