using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Infrastructure.Persistence.Identity
{
    public static class IdentitySeederExtension
    {
        public static async Task SeedRolesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
        }

        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            await IdentitySeeder.SeedAdminUserAsync(scope.ServiceProvider);
        }
    }
}
