using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Queries.GetCartItems
{
    public class GetCartDetailsQueryHandler : IQueryHandler<GetCartDetailsQuery, CartDetailsDto>
    {
        private readonly ICartService _cartService;

        public GetCartDetailsQueryHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result<CartDetailsDto>> Handle(
            GetCartDetailsQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.GetActiveCartDetailsAsync(cancellationToken);
            return result;
        }
    }
}
