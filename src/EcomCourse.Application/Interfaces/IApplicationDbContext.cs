using EcomCourse.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Cart> Carts { get; }
        DbSet<CartItem> CartItems { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
