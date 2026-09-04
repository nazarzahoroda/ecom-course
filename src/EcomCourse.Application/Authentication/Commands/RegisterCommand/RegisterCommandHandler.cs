using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Authentication.Commands.RegisterCommand
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly ICustomerStore _customerStore;
        private readonly CompensateAsync _compensateAsync;
        public RegisterCommandHandler(IIdentityService identityService, ICustomerStore customerStore, CompensateAsync compensateAsync)
        {
            _identityService = identityService;
            _customerStore = customerStore;
            _compensateAsync = compensateAsync;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var exists = await _identityService.IsUserExist(request.dto.Email, cancellationToken);
            if (exists)
            {
                return Result.Failure(new DomainError("Identity.Register", "User already exists"));
            }
            var createResult = await _identityService.CreateUserAsyncWithResult(request.dto, cancellationToken);

            if (createResult.IsFailure)
            {
                return createResult;
            }

            var user = createResult.Value;

            var customerResult = Customer.Create(
                   user!.Id,
                   request.dto.Name,
                   request.dto.Email,
                   request.dto.Street,
                   request.dto.City,
                   request.dto.PostalCode,
                   request.dto.Country);
            if (customerResult.IsFailure)
            {
                var compensateResult = await _compensateAsync.CompensateAsyncTask(user!.Id, Guid.Empty, cancellationToken);
                if (compensateResult.IsFailure)
                    return compensateResult;

                return customerResult;
            }

            var customer = customerResult.Value;

            var wasAdded = await _customerStore.AddAsync(customer!, cancellationToken);


            if (!wasAdded)
            {
                var compensateResult = await _compensateAsync.CompensateAsyncTask(user!.Id, Guid.Empty, cancellationToken);
                if (compensateResult.IsFailure)
                    return compensateResult;

                return Result.Failure(new DomainError("Customer.CreateFailed", "Failed to create customer"));

            }
            var updateUserResult = await _identityService.SetCustomerIdAsync(user!.Id, customer!.Id, cancellationToken);

            if (updateUserResult.IsFailure)
            {
                var compensateResult = await _compensateAsync.CompensateAsyncTask(user!.Id, customer.Id, cancellationToken);
                if (compensateResult.IsFailure)
                    return compensateResult;

                return updateUserResult;
            }

            return Result.Success();
        }
    }
}
