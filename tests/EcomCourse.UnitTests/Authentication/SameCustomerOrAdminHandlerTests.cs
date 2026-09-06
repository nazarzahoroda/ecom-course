using System.Security.Claims;
using EcomCourse.Infrastructure.Persistence.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace EcomCourse.Infrastructure.Tests.Authorization;

public class SameCustomerOrAdminHandlerTests
{
    private readonly SameCustomerOrAdminHandler _handler;
    private readonly SameCustomerOrAdminRequirement _requirement;

    public SameCustomerOrAdminHandlerTests()
    {
        _handler = new SameCustomerOrAdminHandler();
        _requirement = new SameCustomerOrAdminRequirement();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsAdmin_Succeeds()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var resource = new CustomerResource(Guid.NewGuid());
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsSameCustomer_Succeeds()
    {
        var customerId = Guid.NewGuid();
        var claims = new[] { new Claim("CustomerId", customerId.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var resource = new CustomerResource(customerId);
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsDifferentCustomer_DoesNotSucceed()
    {
        var customerId = Guid.NewGuid();
        var resourceCustomerId = Guid.NewGuid();
        var claims = new[] { new Claim("CustomerId", customerId.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var resource = new CustomerResource(resourceCustomerId);
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCustomerIdClaimMissing_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth"));
        var resource = new CustomerResource(Guid.NewGuid());
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCustomerIdClaimInvalid_DoesNotSucceed()
    {
        var claims = new[] { new Claim("CustomerId", "invalid-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var resource = new CustomerResource(Guid.NewGuid());
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
