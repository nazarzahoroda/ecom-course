using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Common
{
    public sealed class ValidationResult : Result, IValidationResult
    {
        private ValidationResult(DomainError[] errors)
            : base(false, IValidationResult.ValidationError) =>
            Errors = errors;

        public DomainError[] Errors { get; }

        public static ValidationResult WithErrors(DomainError[] errors) => new(errors);
    }
}
