using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Carts.Commands.AddItemToCartCommand
{
    public class AddItemToCartCommandHandler : ICommandHandler<AddItemToCartCommand>
    {
        private readonly ICartService _cartService;

        public AddItemToCartCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result> Handle(
            AddItemToCartCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.AddItemToCartAsync(request.dto, cancellationToken);
            return result;
        }
    }
}
