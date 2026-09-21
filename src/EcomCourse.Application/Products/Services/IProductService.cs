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
        CancellationToken cancellationToken = default
    );

    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<Result<IReadOnlyList<ProductDto>>> GetTopAsync(
        CancellationToken cancellationToken = default
    );

    Task<Result> UpdateAsync(
        Guid id,
        string name,
        decimal amount,
        Currency currency,
        string sku,
        Guid categoryId,
        CancellationToken cancellationToken = default
    );
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsProductExists(Guid id, CancellationToken cancellationToken);

    public Task ChangeMainImages(Guid id, CancellationToken cancellationToken);

    public Task<Result<Guid>> AddImage(
        Guid Id,
        string blobName,
        string contentType,
        bool isMain,
        CancellationToken cancellationToken
    );

    public Task<Result<List<ProductImageDto>>> GetProductImagesAsync(
        Guid id,
        CancellationToken cancellationToken
    );

    public Task<Result> DeleteImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken
    );
}
