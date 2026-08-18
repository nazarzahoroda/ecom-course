using EcomCourse.Application.Customers;
using EcomCourse.Application.Customers.GetCustomerById;
using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Application.Customers;

public class GetCustomerByIdQueryHandlerTests
{

    [Fact]
    public async Task HandleReturnsCustomerWhenCustomerExists()
    {
        var repository = new FakeCustomerRepository();
        var userId = Guid.NewGuid();

        var customerResult = Customer.Create(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine");

        await repository.AddAsync(customerResult.Value!, CancellationToken.None);

        var handler = new GetCustomerByIdQueryHandler(repository);

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
        Assert.Equal("Ukraine", result.Value.Address.Country); }


        private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers = [];

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        {
            var exists = _customers.Any(customer => customer.Email.Equals(email));
            return Task.FromResult(exists);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            _customers.Add(customer);
            return Task.CompletedTask;
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = _customers.FirstOrDefault(customer => customer.Id == id);
            return Task.FromResult(customer);
        }
    }
}



