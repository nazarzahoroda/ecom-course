using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EcomCourse.Infrastructure;

namespace EcomCourse.UnitTests;

public class InfrastructureTest
{
    [Fact]
    public void InfrastructureRegistration()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost,1433;Database=EcomDb;User Id=sa;Password=TestPassword123!;TrustServerCertificate=True"
            })
            .Build();

        var exception = Record.Exception(
            () => services.AddInfrastructure(configuration));

        Assert.Null(exception);
    }
}
