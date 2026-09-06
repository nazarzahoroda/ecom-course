using Microsoft.AspNetCore.Identity;

namespace EcomCourse.Infrastructure.Persistence.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid CustomerId { get; set; }
    }

}
