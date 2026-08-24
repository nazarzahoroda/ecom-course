using EcomCourse.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
