using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Customers;

public interface ICustomerRepository
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);

    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
