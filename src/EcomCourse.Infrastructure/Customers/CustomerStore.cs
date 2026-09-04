using EcomCourse.Domain.Customers;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EcomCourse.Infrastructure.Customers;

public sealed class CustomerStore : ICustomerStore
{
    private readonly EcomCourseDbContext _dbContext;

    public CustomerStore(EcomCourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AnyAsync(customer => customer.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        _dbContext.Customers.Add(customer);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            _dbContext.Entry(customer).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .Include(customer => customer.Address)
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer is null)
        {
            return false;
        }
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }
}
