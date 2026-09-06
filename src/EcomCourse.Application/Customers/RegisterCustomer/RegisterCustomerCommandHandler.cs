using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;


namespace EcomCourse.Application.Customers.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler
    : ICommandHandler<RegisterCustomerCommand, Guid>
{


    private readonly ICustomerStore _customerStore;

    public RegisterCustomerCommandHandler (ICustomerStore customerStore)
    {
        _customerStore = customerStore;

    }


    public async Task<Result<Guid>> Handle(
    RegisterCustomerCommand request,
    CancellationToken cancellationToken)
    {


        var customerResult = Customer.Create(
    request.UserId,
    request.Name,
    request.Email,
    request.Street,
    request.City,
    request.PostalCode,
    request.Country);

        if (customerResult.IsFailure)
        {
            return Result.Failure<Guid>(customerResult.Error);
        }

        var emailExists = await _customerStore.ExistsByEmailAsync(customerResult.Value!.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(CustomerErrors.EmailAlreadyExists);
        }

        var wasAdded = await _customerStore.AddAsync(customerResult.Value!, cancellationToken);

        if (!wasAdded)
        {
            return Result.Failure<Guid>(CustomerErrors.EmailAlreadyExists);
        }

        return Result.Success(customerResult.Value!.Id);
    }

}
