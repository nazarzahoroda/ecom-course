
namespace EcomCourse.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; init; }
        public bool IsFailure => !IsSuccess;
        public DomainError Error { get; init; }


        protected Result(bool isSuccess, DomainError error)
        {
            if (isSuccess && error != DomainError.None)
            {
                throw new InvalidOperationException("Success result cannot contain an error.");
            }

            if (!isSuccess && error == DomainError.None)
            {
                throw new InvalidOperationException("Failure result must contain an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new Result(true, DomainError.None);
        }

        public static Result Failure(DomainError error)
        {
            return new Result(false, error);
        }

        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value, true, DomainError.None);
        }

        public static Result<TValue> Failure<TValue>(DomainError error)
        {
            return new Result<TValue>(default, false, error);
        }
    }

    public class Result<TValue> : Result
    {
        public TValue? Value { get; init; }

        protected internal Result(TValue? value, bool isSuccess, DomainError error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}


