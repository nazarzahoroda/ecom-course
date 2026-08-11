using EcomCourse.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EcomCourse.UnitTests.Infrastructure.Persistence;

public class ConnectionStringResolverTests
{
    [Fact]
    public void ResolveShouldReturnConnectionStringWhenConfigured()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=TestDb;"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var result = ConnectionStringResolver.Resolve(configuration);

        Assert.Equal("Server=localhost;Database=TestDb;", result);
    }

    [Fact]
    public void ResolveShouldThrowExceptionWhenMissing()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringResolver.Resolve(configuration));

        Assert.Contains("dotnet user-secrets set", exception.Message);
    }
}