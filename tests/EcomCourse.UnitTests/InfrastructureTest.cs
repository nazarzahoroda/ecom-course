using EcomCourse.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.UnitTests;

public class InfrastructureTest
{
    [Fact]
    public void InfrastructureRegistration()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Server=localhost,1433;Database=EcomDb;User Id=sa;Password=TestPassword123!;TrustServerCertificate=True",
                    ["Jwt:Key"] = "super_secret_key_for_unit_tests_1234567890!",
                    ["Jwt:Issuer"] = "EcomCourse",
                    ["Jwt:Audience"] = "EcomCourse",
                }
            )
            .Build();

        var exception = Record.Exception(() => services.AddInfrastructure(configuration));

        Assert.Null(exception);
    }
}
