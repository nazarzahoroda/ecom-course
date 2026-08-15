using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Common
{
    public interface IValidationResult
    {
        public static readonly DomainError ValidationError = new(
            "ValidationError",
            "A validation problem occurred.");

        DomainError[] Errors { get; }
    }
}
