using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Authentication;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Customers;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.Infrastructure.Persistence.Repositories;
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

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Authentication
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IIdentityProvider, IdentityProvider>();

        // User context
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, EcomCourse.Infrastructure.Persistence.Repositories.OrderRepository>();

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Application managers
        services.AddScoped<ICartManager, CartManager>();
        services.AddScoped<ICategoryManager, CategoryManager>();
        services.AddScoped<IProductManager, ProductManager>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
