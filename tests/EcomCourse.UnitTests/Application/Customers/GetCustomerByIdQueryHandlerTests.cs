using EcomCourse.Application.Customers.GetCustomerById;
using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Application.Customers;

public class GetCustomerByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleReturnsFailureWhenCustomerDoesNotExist()
    {
        var store = new FakeCustomerStore();
        var handler = new GetCustomerByIdQueryHandler(store);

        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task HandleReturnsCustomerWhenCustomerExists()
    {
        var store = new FakeCustomerStore();
        var userId = Guid.NewGuid();

        var customerResult = Customer.Create(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine"
        );

        await store.AddAsync(customerResult.Value!, CancellationToken.None);

        var handler = new GetCustomerByIdQueryHandler(store);

        var query = new GetCustomerByIdQuery(customerResult.Value!.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customerResult.Value.Id, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("Ivan", result.Value.Name);
        Assert.Equal("ivan@example.com", result.Value.Email);
        Assert.Equal("Polubotka", result.Value.Address.Street);
        Assert.Equal("Lviv", result.Value.Address.City);
        Assert.Equal("79066", result.Value.Address.PostalCode);
        Assert.Equal("Ukraine", result.Value.Address.Country);
    }

    private sealed class FakeCustomerStore : ICustomerStore
    {
        private readonly List<Customer> _customers = [];

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        {
            var exists = _customers.Any(customer => customer.Email.Equals(email));
            return Task.FromResult(exists);
        }

        public Task<bool> AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            _customers.Add(customer);
            return Task.FromResult(true);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = _customers.FirstOrDefault(customer => customer.Id == id);
            return Task.FromResult(customer);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var removedCount = _customers.RemoveAll(customer => customer.Id == id);
            return Task.FromResult(removedCount > 0);
        }
    }
}
