using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand
{
    public class UpdateCartItemQuantityCommandHandler
        : ICommandHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartManager _cartManager;

        public UpdateCartItemQuantityCommandHandler(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<Result> Handle(
            UpdateCartItemQuantityCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartManager.UpdateCartItemQuantityAsync(
                request.dto,
                cancellationToken
            );
            return result;
        }
    }
}
