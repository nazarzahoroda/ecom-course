using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace EcomCourse.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;
        public DomainError Error { get; set; }

       
        public Result(bool isSuccess, DomainError error)
        {
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
        public TValue? Value { get; set; }

        public Result(TValue? value, bool isSuccess, DomainError error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}


