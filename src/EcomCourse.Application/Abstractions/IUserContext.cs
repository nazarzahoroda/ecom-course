namespace EcomCourse.Application.Abstractions
{
    public interface IUserContext
    {
        Guid UserId { get; }
        Guid CustomerId { get; }
        bool IsAuthenticated { get; }
    }
}
