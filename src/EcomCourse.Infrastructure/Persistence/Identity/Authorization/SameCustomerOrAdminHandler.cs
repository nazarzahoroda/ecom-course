using Microsoft.AspNetCore.Authorization;

namespace EcomCourse.Infrastructure.Persistence.Identity.Authorization
{
    public record CustomerResource(Guid CustomerId);
    public class SameCustomerOrAdminHandler
        : AuthorizationHandler<
            SameCustomerOrAdminRequirement,
            CustomerResource>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameCustomerOrAdminRequirement requirement,
            CustomerResource resource)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var customerIdClaim =
                context.User.FindFirst("CustomerId");

            if (customerIdClaim is null)
            {
                return Task.CompletedTask;
            }

            if (!Guid.TryParse(
                    customerIdClaim.Value,
                    out var customerId))
            {
                return Task.CompletedTask;
            }

            if (customerId == resource.CustomerId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
