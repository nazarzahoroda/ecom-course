using EcomCourse.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly EcomCourseDbContext _context;

    public CartRepository(EcomCourseDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetActiveCartByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == CartStatus.Active, cancellationToken);
    }
}