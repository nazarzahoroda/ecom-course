using Microsoft.AspNetCore.Authorization;

namespace EcomCourse.Infrastructure.Authorization
{
    public class SameCustomerOrAdminRequirement : IAuthorizationRequirement { }
}
