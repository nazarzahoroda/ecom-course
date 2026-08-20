using EcomCourse.Application.Customers.RegisterCustomer;
using EcomCourse.Domain.Customers;

namespace EcomCourse.UnitTests.Application.Customers;

public class RegisterCustomerCommandHandlerTests
{
    [Fact]
    public async Task HandleReturnsSuccessWhenCommandIsValid()
    {
        var store = new FakeCustomerStore();
        var handler = new RegisterCustomerCommandHandler(store);
        var userId = Guid.NewGuid();

        var command = new RegisterCustomerCommand(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }


    [Fact]
    public async Task HandleReturnsFailureWhenEmailAlreadyExists()
    {
        var store = new FakeCustomerStore();
        var handler = new RegisterCustomerCommandHandler(store);
        var userId = Guid.NewGuid();

        var command = new RegisterCustomerCommand(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine");

        var firstResult = await handler.Handle(command, CancellationToken.None);
        var secondResult = await handler.Handle(command, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);
        Assert.Equal(CustomerErrors.EmailAlreadyExists, secondResult.Error);
    }

    [Fact]
    public async Task HandleReturnsFailureWhenEmailIsInvalid()
    {
        var store = new FakeCustomerStore();
        var handler = new RegisterCustomerCommandHandler(store);

        var command = new RegisterCustomerCommand(
            Guid.NewGuid(),
            "Ivan",
            "invalid-email",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.EmailInvalidFormat, result.Error);
    }

    [Fact]
    public async Task HandleReturnsFailureWhenAddFailsBecauseEmailAlreadyExists()
    {
        var store = new FakeCustomerStore(addResult: false);
        var handler = new RegisterCustomerCommandHandler(store);

        var command = new RegisterCustomerCommand(
            Guid.NewGuid(),
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.EmailAlreadyExists, result.Error);
    }

    private sealed class FakeCustomerStore : ICustomerStore
    {
        private readonly List<Customer> _customers = [];
        private readonly bool _addResult;

        public FakeCustomerStore(bool addResult = true)
        {
            _addResult = addResult;
        }

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        {
            var exists = _customers.Any(customer => customer.Email.Equals(email));

            return Task.FromResult(exists);
        }

        public Task<bool> AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            if (!_addResult)
            {
                return Task.FromResult(false);
            }

            _customers.Add(customer);

            return Task.FromResult(true);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = _customers.FirstOrDefault(customer => customer.Id == id);

            return Task.FromResult(customer);
        }
    }


}
