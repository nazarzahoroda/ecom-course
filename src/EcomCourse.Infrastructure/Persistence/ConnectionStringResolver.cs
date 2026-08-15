using Microsoft.Extensions.Configuration;

namespace EcomCourse.Infrastructure.Persistence;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {

            var fallback = "Server=(localdb)\\mssqllocaldb;Database=EcomCourseDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            return fallback;
        }

        return connectionString;
    }
}
