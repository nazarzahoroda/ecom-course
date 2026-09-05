using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand
{
    public class RemoveItemFromCartCommandHandler : ICommandHandler<RemoveItemFromCartCommand, Guid>
    {
        private readonly ICartService _cartService;

        public RemoveItemFromCartCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result<Guid>> Handle(
            RemoveItemFromCartCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.RemoveItemFromCartAsync(request.id, cancellationToken);
            return result;
        }
    }
}
