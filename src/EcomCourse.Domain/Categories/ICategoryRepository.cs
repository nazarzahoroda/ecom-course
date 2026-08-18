using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Categories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

        void Add(Category category);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
