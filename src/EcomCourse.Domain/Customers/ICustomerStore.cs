namespace EcomCourse.Domain.Customers;

public interface ICustomerStore
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<bool> AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
