using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Customers;

public sealed class Customer : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Address Address { get; private set; }



    private Customer()
    : base(Guid.Empty)
    {
        UserId = Guid.Empty;
        Name = string.Empty;
        Email = null!;
        Address = null!;
    }
    private Customer(Guid id, Guid userId, string name, Email email, Address address) : base(id)
    {
        UserId = userId;
        Name = name;
        Email = email;
        Address = address;
    }


    public static Result<Customer> Create(
    Guid userId,
    string name,
    string email,
    string street,
    string city,
    string postalCode,
    string country)
    {

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Customer>(CustomerErrors.NameRequired);
        }

        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<Customer>(emailResult.Error);
        }

        var addressResult = Address.Create(street, city, postalCode, country);

        if (addressResult.IsFailure)
        {
            return Result.Failure<Customer>(addressResult.Error);
        }

        return Result.Success(new Customer(
        Guid.NewGuid(),
        userId,
        name.Trim(),
        emailResult.Value!,
        addressResult.Value!));

    }

}


