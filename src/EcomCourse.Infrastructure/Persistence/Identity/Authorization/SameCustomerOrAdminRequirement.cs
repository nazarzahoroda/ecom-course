using Microsoft.AspNetCore.Authorization;

namespace EcomCourse.Infrastructure.Persistence.Identity.Authorization
{
    public class SameCustomerOrAdminRequirement : IAuthorizationRequirement
    {
    }
}
