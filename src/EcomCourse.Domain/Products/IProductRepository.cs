namespace EcomCourse.Domain.Products;

public interface IProductRepository
{
    Task<Dictionary<Guid, decimal>> GetProductPricesAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}