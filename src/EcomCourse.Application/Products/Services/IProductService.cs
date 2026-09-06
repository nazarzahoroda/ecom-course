using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Products.Services;

public interface IProductService
{
    Task<Result<Guid>> CreateAsync(
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
