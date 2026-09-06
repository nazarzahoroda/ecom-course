using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Customers.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    Guid UserId,
    string Name,
    string Email,
    string Street,
    string City,
    string PostalCode,
    string Country) : ICommand<Guid>;
