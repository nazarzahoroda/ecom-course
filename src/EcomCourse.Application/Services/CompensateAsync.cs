using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Customers;
using MediatR;

namespace EcomCourse.Application.Services
{
    public class CompensateAsync
    {
        private readonly ICustomerStore _customerStore;
        private readonly IIdentityService _identityService;

        public CompensateAsync(ICustomerStore customerStore, IIdentityService identityService)
        {
            _customerStore = customerStore;
            _identityService = identityService;
        }

        public async Task CompensateAsyncTask(Guid userId, Guid customerId, CancellationToken cancellationToken)
        {
            if (customerId != Guid.Empty)
            {
                var customer = await _customerStore.GetByIdAsync(customerId, cancellationToken);

                if (customer is not null)
                {
                    var deleteResult = await _customerStore.DeleteAsync(customer.Id, cancellationToken);
                    if (!deleteResult)
                    {
                        throw new InvalidOperationException(
                            $"Failed to delete customer with ID: {customerId}");
                    }
                }

                
            }
            await _identityService.DeleteUserAsync(userId, cancellationToken);
        }
    }
}   
