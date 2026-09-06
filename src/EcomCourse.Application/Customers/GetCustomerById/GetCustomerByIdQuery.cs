using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Customers.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerResponse>;
