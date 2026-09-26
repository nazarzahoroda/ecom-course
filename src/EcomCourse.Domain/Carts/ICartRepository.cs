namespace EcomCourse.Domain.Carts;

public interface ICartRepository
{
    Task<Cart?> GetActiveCartByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}