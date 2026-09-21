using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcomCourse.IntegrationTests.TestSupport;

// Swaps the real JWT bearer scheme for a header-driven fake, so integration tests
// don't need to sign tokens against the developer's local Jwt:Key/Issuer/Audience secrets.
public static class TestAuthHandler
{
    public const string SchemeName = "TestScheme";
    private const string CustomerIdHeader = "X-Test-CustomerId";
    private const string RoleHeader = "X-Test-Role";
    private const string AnonymousHeader = "X-Test-Anonymous";

    public static WebApplicationFactory<TEntryPoint> WithTestAuthentication<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory
    )
        where TEntryPoint : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = SchemeName;
                        options.DefaultChallengeScheme = SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, Handler>(SchemeName, _ => { });
            });
        });
    }

    public static void AuthenticateAs(this HttpClient client, Guid customerId, string? role = null)
    {
        client.DefaultRequestHeaders.Remove(CustomerIdHeader);
        client.DefaultRequestHeaders.Add(CustomerIdHeader, customerId.ToString());

        client.DefaultRequestHeaders.Remove(RoleHeader);
        if (role is not null)
        {
            client.DefaultRequestHeaders.Add(RoleHeader, role);
        }
    }

    public static void AuthenticateAsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(AnonymousHeader);
        client.DefaultRequestHeaders.Add(AnonymousHeader, "true");
    }

    private sealed class Handler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public Handler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder
        )
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (
                Request.Headers.TryGetValue(AnonymousHeader, out var anonymous)
                && anonymous == "true"
            )
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (
                !Request.Headers.TryGetValue(CustomerIdHeader, out var customerIdHeader)
                || !Guid.TryParse(customerIdHeader, out var customerId)
            )
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim> { new("CustomerId", customerId.ToString()) };

            if (Request.Headers.TryGetValue(RoleHeader, out var role) && role.Count > 0)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
