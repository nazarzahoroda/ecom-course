using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Primitives;



namespace EcomCourse.UnitTests.Domain.Primitives
{
    public class Address : ValueObject
    {

        public string City { get; } = "";
        public string Street { get; } = "";

        public Address(string city, string street)
        {

            City = city;
            Street = street;

        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return City;
            yield return Street;
        }

    }
}
