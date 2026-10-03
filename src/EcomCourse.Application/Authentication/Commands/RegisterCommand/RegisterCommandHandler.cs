using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Authentication.Commands.RegisterCommand
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly IIdentityProvider _identityProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(
            IIdentityProvider identityProvider,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork)
        {
            _identityProvider = identityProvider;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _identityProvider.IsUserExist(
                request.dto.Email,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(
                    new DomainError(
                        "Identity.RegistrationFailed",
                        "Registration failed: user already exists",
                        ErrorType.Conflict
                    )
                );
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var createResult = await _identityProvider.CreateUserAsyncWithResult(
                    request.dto,
                    cancellationToken);

                if (createResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return createResult;
                }

                var user = createResult.Value!;

                var customerResult = Customer.Create(
                    user.Id,
                    request.dto.Name,
                    request.dto.Email,
                    request.dto.Street,
                    request.dto.City,
                    request.dto.PostalCode,
                    request.dto.Country
                );

                if (customerResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return customerResult;
                }

                var customer = customerResult.Value!;

                var wasAdded = await _customerRepository.AddAsync(
                    customer,
                    cancellationToken);

                if (!wasAdded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                    return Result.Failure(
                        new DomainError(
                            "Customer.CreateFailed",
                            "Failed to create customer",
                            ErrorType.Failure
                        )
                    );
                }

                var updateUserResult = await _identityProvider.SetCustomerIdAsync(
                    user.Id,
                    customer.Id,
                    cancellationToken);

                if (updateUserResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return updateUserResult;
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
