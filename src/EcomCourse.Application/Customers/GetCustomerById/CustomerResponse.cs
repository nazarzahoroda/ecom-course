namespace EcomCourse.Application.Customers.GetCustomerById;

public sealed record CustomerResponse(Guid Id, string Name, string Email, AddressResponse Address);

public sealed record AddressResponse(string Street, string City, string PostalCode, string Country);
