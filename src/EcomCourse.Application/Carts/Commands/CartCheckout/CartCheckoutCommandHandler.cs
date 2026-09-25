using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.CartCheckout
{
    public class CartCheckoutCommandHandler : ICommandHandler<CartCheckoutCommand, Guid>
    {
        private readonly ICartManager _cartManager;

        public CartCheckoutCommandHandler(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<Result<Guid>> Handle(
            CartCheckoutCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartManager.CheckoutCart(cancellationToken);
            return result;
        }
    }
}
