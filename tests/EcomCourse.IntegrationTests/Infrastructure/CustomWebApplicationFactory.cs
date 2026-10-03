using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Respawn;
using Testcontainers.MsSql;

namespace EcomCourse.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory<TProgram, TAppDbContext, TIdentityDbContext>
    : WebApplicationFactory<TProgram>,
        IAsyncLifetime
    where TProgram : class
    where TAppDbContext : DbContext
    where TIdentityDbContext : DbContext
{
    private const string _jwtSecretKey = "Very_Super_Puper_Secret_Key123!321";

    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-latest"
    )
        .WithPassword("Strong_P@ssw0rd!")
        .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var connectionString = _dbContainer.GetConnectionString();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            connectionString
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__IdentityConnection",
            connectionString
        );

        Environment.SetEnvironmentVariable("Jwt__Key", _jwtSecretKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "EcomCourse");
        Environment.SetEnvironmentVariable("Jwt__Audience", "EcomCourseClient");

        var identityOptions = new DbContextOptionsBuilder<TIdentityDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using (
            var identityDb = (TIdentityDbContext)
                Activator.CreateInstance(typeof(TIdentityDbContext), identityOptions)!
        )
        {
            await identityDb.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'identity') EXEC('CREATE SCHEMA [identity]')"
            );
            await identityDb.Database.MigrateAsync();
        }

        var appOptions = new DbContextOptionsBuilder<TAppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using (
            var appDb = (TAppDbContext)Activator.CreateInstance(typeof(TAppDbContext), appOptions)!
        )
        {
            await appDb.Database.MigrateAsync();
        }

        _dbConnection = new SqlConnection(connectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"],
                SchemasToInclude = ["dbo", "identity"],
            }
        );
    }

    /// <summary>
    /// Метод швидкого очищення даних у БД між окремими тестами.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connectionString = _dbContainer.GetConnectionString();

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString,
                        ["ConnectionStrings:IdentityConnection"] = connectionString,
                        ["Jwt:Key"] = _jwtSecretKey,
                        ["Jwt:Issuer"] = "EcomCourse",
                        ["Jwt:Audience"] = "EcomCourseClient",
                        ["Jwt:AccessTokenMinutes"] = "60",
                        ["Jwt:RefreshTokenDays"] = "7",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TIdentityDbContext>>();
            services.AddDbContext<TIdentityDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.RemoveAll<DbContextOptions<TAppDbContext>>();
            services.AddDbContext<TAppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
        }

        await _dbContainer.StopAsync();
    }
}
