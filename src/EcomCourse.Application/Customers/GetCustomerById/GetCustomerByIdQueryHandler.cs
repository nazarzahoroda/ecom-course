using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Customers.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
{

    private readonly ICustomerStore _customerStore;

    public GetCustomerByIdQueryHandler(ICustomerStore customerStore)
    {
        _customerStore = customerStore;
    }

    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {

        var customer = await _customerStore.GetByIdAsync(request.CustomerId, cancellationToken);


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
