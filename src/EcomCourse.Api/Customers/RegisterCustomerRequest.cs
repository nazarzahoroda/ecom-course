namespace EcomCourse.Api.Customers;


public sealed record RegisterCustomerRequest(
    Guid UserId,
    string Name,
    string Email,
    string Street,
    string City,
    string PostalCode,
    string Country);
