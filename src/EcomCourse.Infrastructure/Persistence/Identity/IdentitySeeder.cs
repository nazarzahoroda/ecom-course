using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.Infrastructure.Persistence.Identity
{
    public static class IdentitySeeder
    {
        private static readonly string[] _roles = { "Admin", "Customer" };

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var role in _roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(x => x.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {errors}"
                    );
                }
            }
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var adminEmail =
                configuration["SeedAdmin:Email"]
                ?? throw new InvalidOperationException("SeedAdmin:Email is not configured.");

            var adminPassword =
                configuration["SeedAdmin:Password"]
                ?? throw new InvalidOperationException("SeedAdmin:Password is not configured.");

            var existingUser = await userManager.FindByEmailAsync(adminEmail);

            if (existingUser is null)
            {
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }

                var addRolesResult = await userManager.AddToRolesAsync(adminUser, _roles);

                if (!addRolesResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        addRolesResult.Errors.Select(x => x.Description)
                    );
                    throw new InvalidOperationException(
                        $"Failed to assign roles to admin user: {errors}"
                    );
                }
            }
            else
            {
                foreach (var role in _roles)
                {
                    if (!await userManager.IsInRoleAsync(existingUser, role))
                    {
                        await userManager.AddToRoleAsync(existingUser, role);
                    }
                }
            }
        }
    }
}
