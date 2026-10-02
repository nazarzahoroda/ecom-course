using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;

namespace EcomCourse.Application.Authentication.Commands.RegisterCommand
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly ICustomerStore _customerStore;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(
            IIdentityService identityService,
            ICustomerStore customerStore,
            IUnitOfWork unitOfWork
        )
        {
            _identityService = identityService;
            _customerStore = customerStore;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken
        )
        {
            var exists = await _identityService.IsUserExist(
                request.dto.Email,
                cancellationToken
            );

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
                var createResult = await _identityService.CreateUserAsyncWithResult(
                    request.dto,
                    cancellationToken
                );

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

                var wasAdded = await _customerStore.AddAsync(
                    customer,
                    cancellationToken
                );

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

                var updateUserResult = await _identityService.SetCustomerIdAsync(
                    user.Id,
                    customer.Id,
                    cancellationToken
                );

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
