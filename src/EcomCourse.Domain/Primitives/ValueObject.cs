using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Primitives
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {

        protected abstract IEnumerable<object> GetEqualityComponents();

        public bool Equals(ValueObject? other)
        {
            if (other == null)
            {
                return false;
            }

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

        }


        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ValueObject)obj);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }
    }
}
