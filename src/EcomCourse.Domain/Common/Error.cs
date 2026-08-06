using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Common
{
    public record DomainError(string Code, string Description)
    {
        public static readonly DomainError None = new("", "");
        public static readonly DomainError NullValue = new("Error.NullValue", "Значення не може бути null.");
    }
}
