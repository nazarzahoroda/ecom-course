using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Customers.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
{

    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);


        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        var response = new CustomerResponse(
    customer.Id,
    customer.UserId,
    customer.Name,
    customer.Email.Value,
    new AddressResponse(
        customer.Address.Street,
        customer.Address.City,
        customer.Address.PostalCode,
        customer.Address.Country));

        return Result.Success(response);

    }


}
