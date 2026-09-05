namespace EcomCourse.Application.Interfaces
{
    public interface IUserContext
    {
        Guid UserId { get; }
        Guid CustomerId { get; }
        bool IsAuthenticated { get; }
    }
}
