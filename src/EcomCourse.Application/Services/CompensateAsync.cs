using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

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

        public async Task<Result> CompensateAsyncTask(Guid userId, Guid customerId, CancellationToken cancellationToken)
        {
            if (customerId != Guid.Empty)
            {
                var customer = await _customerStore.GetByIdAsync(customerId, cancellationToken);

                if (customer is not null)
                {
                    var deleteCustomerResult = await _customerStore.DeleteAsync(customer.Id, cancellationToken);
                    if (!deleteCustomerResult)
                    {
                        return Result.Failure(new DomainError("Compensation.CustomerDeleteFailed", "Failed to delete customer"));
                    }
                }
            }
            var deleteUserResult = await _identityService.DeleteUserAsync(userId, cancellationToken);
            if (deleteUserResult.IsFailure)
                return deleteUserResult;

            return Result.Success();
        }
    }
}   
