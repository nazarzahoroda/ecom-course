using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Carts.Commands.AddItemToCartCommand
{
    public class AddItemToCartCommandHandler : ICommandHandler<AddItemToCartCommand>
    {
        private readonly ICartManager _cartManager;

        public AddItemToCartCommandHandler(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<Result> Handle(
            AddItemToCartCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartManager.AddItemToCartAsync(request.dto, cancellationToken);
            return result;
        }
    }
}
