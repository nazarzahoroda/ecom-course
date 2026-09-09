using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Carts.Commands.CartCheckout
{
    public record CartCheckoutCommand() : ICommand<Guid>;
}
