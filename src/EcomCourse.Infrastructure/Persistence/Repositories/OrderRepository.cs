using EcomCourse.Domain.Orders;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcomCourseDbContext _context;

        public OrderRepository(EcomCourseDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetByCustomerIdAsync(
            Guid customerId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Orders.Where(o => o.CustomerId == customerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }
    }
}