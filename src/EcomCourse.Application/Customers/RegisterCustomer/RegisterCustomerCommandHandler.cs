using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;


namespace EcomCourse.Application.Customers.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler
    : ICommandHandler<RegisterCustomerCommand, Guid>
{


    private readonly ICustomerRepository _customerRepository;

    public RegisterCustomerCommandHandler (ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

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

        var emailExists = await _customerRepository.ExistsByEmailAsync(customerResult.Value!.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(CustomerErrors.EmailAlreadyExists);
        }

        await _customerRepository.AddAsync(customerResult.Value!, cancellationToken);

        return Result.Success(customerResult.Value!.Id);
    }

}
