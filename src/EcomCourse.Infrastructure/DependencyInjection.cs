using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Infrastructure.Customers;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EcomCourse.Application.Categories.Services;
using EcomCourse.Infrastructure.Services;

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
        services.AddScoped<ICustomerStore, CustomerStore>();
	services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
