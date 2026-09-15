using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Queries.GetCartItems
{
    public class GetCartItemsQueryHandler : IQueryHandler<GetCartItemsQuery, CartDetailsDto>
    {
        private readonly ICartService _cartService;

        public GetCartItemsQueryHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<Result<CartDetailsDto>> Handle(
            GetCartItemsQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartService.GetActiveCartDetailsAsync(cancellationToken);
            return result;
        }
    }
}
