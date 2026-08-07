namespace EcomCourse.Api
{
    public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

    public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto>
    {
        public async Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            // Імітація отримання з БД
            var user = new UserDto(query.Id, "Іван", "ivan@example.com");
            return await Task.FromResult(user);
        }
    }

    public record UserDto(Guid Id, string Name, string Email);
}
