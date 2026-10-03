namespace EcomCourse.Application.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(Exception innerException)
        : base("A concurrency conflict occurred.", innerException)
    {
    }
}
