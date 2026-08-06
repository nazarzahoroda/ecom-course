namespace EcomCourse.Api
{
    //public class GetUserQuery
    //{
    //    public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

    //    public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto>
    //    {
    //        public async Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    //        {
    //            // Імітація отримання з БД
    //            var user = new UserDto(query.Id, "Test User", "test@example.com");
    //            return await Task.FromResult(user);
    //        }
    //    }

    //    public record UserDto(Guid Id, string Name, string Email);
    //}

    public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

    public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto>
    {
        public async Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
        {
            // Імітація отримання з БД
            var user = new UserDto(query.Id, "Іван", "ivan@example.com");
            return await Task.FromResult(user);
        }
    }

    public record UserDto(Guid Id, string Name, string Email);
}
