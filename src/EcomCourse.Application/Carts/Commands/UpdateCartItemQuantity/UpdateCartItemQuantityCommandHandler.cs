using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand
{
    public class UpdateCartItemQuantityCommandHandler
        : ICommandHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartService _cartService;

        public UpdateCartItemQuantityCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result> Handle(
            UpdateCartItemQuantityCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.UpdateCartItemQuantityAsync(
                request.dto,
                cancellationToken
            );
            return result;
        }
    }
}
