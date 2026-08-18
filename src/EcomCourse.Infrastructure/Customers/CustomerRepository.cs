using EcomCourse.Application.Customers;
using EcomCourse.Domain.Customers;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Customers;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AnyAsync(customer => customer.Email.Value == email.Value, cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        _dbContext.Customers.Add(customer);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .Include(customer => customer.Address)
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }
}
