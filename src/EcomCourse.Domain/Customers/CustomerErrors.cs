
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Customers
{
    public static class CustomerErrors
    {

        public static readonly DomainError EmailRequired = new("Customer.EmailRequired", "Email is required.");




        public static readonly DomainError EmailInvalidFormat = new("Customer.EmailInvalidFormat", "Email format is invalid.");
    


    public static readonly DomainError StreetRequired = new("Customer.StreetRequired", "Street is required.");
    
        public static readonly DomainError CityRequired = new("Customer.CityRequired", "City is required.");

        public static readonly DomainError PostalCodeRequired = new("Customer.PostalCodeRequired", "Postal code is required.");

        public static readonly DomainError CountryRequired = new("Customer.CountryRequired", "Country is required.");


        public static readonly DomainError NameRequired = new("Customer.NameRequired", "Name is required.");

        public static readonly DomainError EmailAlreadyExists = new("Customer.EmailAlreadyExists", "Customer with this email already exists.");


        public static readonly DomainError NotFound = new("Customer.NotFound", "Customer was not found.");
        public static readonly DomainError BadRequest = new("Customer.BadRequest", "Bad request.");

    }
}
