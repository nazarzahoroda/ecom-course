using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Customers
{
    public sealed class Email : ValueObject
    {

        private static readonly Regex _emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);



        private Email()
        {
            Value = string.Empty;
        }

        private Email(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }


        public static Result<Email> Create(string value)

        {


            if (string.IsNullOrWhiteSpace(value))
            {

                return Result.Failure<Email>(CustomerErrors.EmailRequired);

            }


            var normalizedEmail = value.Trim().ToLowerInvariant();

            if (!_emailRegex.IsMatch(normalizedEmail))
            {
                return Result.Failure<Email>(CustomerErrors.EmailInvalidFormat);
            }

            return Result.Success(new Email(normalizedEmail));


        }


        protected override IEnumerable<object> GetEqualityComponents()
    {
            yield return Value;
        }




    }

}
