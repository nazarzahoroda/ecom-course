using EcomCourse.Application.Authentication.Interfaces;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Infrastructure.Customers;
using EcomCourse.Infrastructure.Interfaces;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
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
        {
            options.UseSqlServer(connectionString);
        });

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddJWTAuth(configuration);

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
        }).AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddSignInManager().AddDefaultTokenProviders();

        services.AddScoped<IJwtService, JwtService>();
            
        services.AddScoped<IApplicationDbContext, EcomCourseDbContext>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerStore, CustomerStore>();

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
