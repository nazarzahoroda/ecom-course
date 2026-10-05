using EcomCourse.Application.Categories;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Abstractions;

public interface ICategoryManager
{
    Task<Result<Guid>> CreateAsync(string name, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<List<CategoryDto>>> GetTopAsync(CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CategoryExists(Guid id, CancellationToken cancellationToken);
    Task<Result> AddImageAsync(
        Guid categoryId,
        string blobName,
        CancellationToken cancellationToken
    );
    Task<Result> DeleteImageAsync(Guid categoryId, CancellationToken cancellationToken);
}
