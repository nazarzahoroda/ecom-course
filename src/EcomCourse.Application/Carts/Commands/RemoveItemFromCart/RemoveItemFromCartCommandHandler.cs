using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand
{
    public class RemoveItemFromCartCommandHandler : ICommandHandler<RemoveItemFromCartCommand, Guid>
    {
        private readonly ICartManager _cartManager;

        public RemoveItemFromCartCommandHandler(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<Result<Guid>> Handle(
            RemoveItemFromCartCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartManager.RemoveItemFromCartAsync(request.id, cancellationToken);
            return result;
        }
    }
}
