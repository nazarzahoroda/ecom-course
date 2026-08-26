using EcomCourse.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

public sealed class OrderRepository : IOrderRepository
{
    private readonly EcomCourseDbContext _dbContext;

    public OrderRepository(EcomCourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(
                o => o.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }



    public async Task UpdateAsync(
    Order order,
    CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
