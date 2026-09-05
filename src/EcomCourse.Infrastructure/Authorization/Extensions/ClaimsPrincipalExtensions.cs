using System.Security.Claims;

namespace EcomCourse.Infrastructure.Authorization.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal? principal)
        {
            string? id = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(id, out Guid parsedId)
                ? parsedId
                : throw new UnauthorizedAccessException("User id is unavailable");
        }

        public static Guid GetCustomerId(this ClaimsPrincipal? principal)
        {
            string? id = principal?.FindFirstValue("CustomerId");

            return Guid.TryParse(id, out Guid parsedId)
                ? parsedId
                : throw new UnauthorizedAccessException("Customer id is unavailable");
        }
    }
}
