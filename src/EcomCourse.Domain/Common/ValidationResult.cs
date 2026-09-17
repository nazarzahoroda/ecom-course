namespace EcomCourse.Domain.Common
{
    public interface IValidationResult
    {
        public static readonly DomainError ValidationError = new(
            "Validation.Error",
            "A validation error occurred.");

        DomainError[] Errors { get; }
    }

    public sealed class ValidationResult : Result, IValidationResult
    {
        private ValidationResult(DomainError[] errors)
            : base(false, IValidationResult.ValidationError) =>
            Errors = errors;

        public DomainError[] Errors { get; }

        public static ValidationResult WithErrors(DomainError[] errors) => new(errors);
    }

    public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
    {
        private ValidationResult(DomainError[] errors)
            : base(default, false, IValidationResult.ValidationError) =>
            Errors = errors;

        public DomainError[] Errors { get; }

        public static ValidationResult<TValue> WithErrors(DomainError[] errors) => new(errors);
    }
}
