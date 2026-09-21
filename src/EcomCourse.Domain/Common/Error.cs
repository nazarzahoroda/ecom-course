namespace EcomCourse.Domain.Common
{
    public record DomainError(
        string Code,
        string Description,
        ErrorType Type)
    {
        public static readonly DomainError None = new(
            "",
            "",
            ErrorType.None);

        public static readonly DomainError NullValue = new(
            "Error.NullValue",
            "Value cannot be null",
            ErrorType.Validation);
    }
}
