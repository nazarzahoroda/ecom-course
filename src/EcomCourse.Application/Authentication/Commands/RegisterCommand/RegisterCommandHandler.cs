using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Authentication.Commands.RegisterCommand
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly IIdentityProvider _identityProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly CompensateAsync _compensateAsync;
        public RegisterCommandHandler(IIdentityProvider identityProvider, ICustomerRepository customerRepository, CompensateAsync compensateAsync)
        {
            _identityProvider = identityProvider;
            _customerRepository = customerRepository;
            _compensateAsync = compensateAsync;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var exists = await _identityProvider.IsUserExist(request.dto.Email, cancellationToken);
            if (exists)
            {
                return Result.Failure(
                    new DomainError(
                        "Identity.Register",
                        "User already exists",
                        ErrorType.Conflict));
            }
            var createResult = await _identityProvider.CreateUserAsyncWithResult(request.dto, cancellationToken);

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

            var wasAdded = await _customerRepository.AddAsync(customer!, cancellationToken);


            if (!wasAdded)
            {
                var compensateResult = await _compensateAsync.CompensateAsyncTask(user!.Id, Guid.Empty, cancellationToken);
                if (compensateResult.IsFailure)
                    return compensateResult;

                return Result.Failure(
                    new DomainError(
                        "Customer.CreateFailed",
                        "Failed to create customer",
                        ErrorType.Failure));

            }
            var updateUserResult = await _identityProvider.SetCustomerIdAsync(user!.Id, customer!.Id, cancellationToken);

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
