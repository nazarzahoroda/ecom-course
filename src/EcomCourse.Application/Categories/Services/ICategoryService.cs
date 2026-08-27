using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Categories.Services;

public interface ICategoryService
{
    Task<Result<Guid>> CreateAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);

    Task<Result<List<CategoryDto>>> GetAllAsync(
    CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(
    Guid id,
    string name,
    CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
    Guid id,
    CancellationToken cancellationToken = default);
}
