using EcomCourse.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly EcomCourseDbContext _context;

    public ProductRepository(EcomCourseDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<Guid, decimal>> GetProductPricesAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price.Amount, cancellationToken);
    }
}