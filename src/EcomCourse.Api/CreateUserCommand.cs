using Microsoft.AspNetCore.Identity;

namespace EcomCourse.Api
{


    public record CreateUserCommand(string Name, string Email) : ICommand<Guid>;

    public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid>
    {
        public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            // Імітація створення користувача в БД
            Guid newUserId = Guid.NewGuid();
            Console.WriteLine($"User created: {command.Name} ({command.Email}) створений з ID: {newUserId}");
            return await Task.FromResult(newUserId);
        }
    }

}
