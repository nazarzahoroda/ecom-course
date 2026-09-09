using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.CartCheckout
{
    public class CartCheckoutCommandHandler : ICommandHandler<CartCheckoutCommand, Guid>
    {
        private readonly ICartService _cartService;

        public CartCheckoutCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result<Guid>> Handle(
            CartCheckoutCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.CheckoutCart(cancellationToken);
            return result;
        }
    }
}
