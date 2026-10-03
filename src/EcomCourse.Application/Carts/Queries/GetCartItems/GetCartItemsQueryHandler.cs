using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Carts.Queries.GetCartItems
{
    public class GetCartDetailsQueryHandler : IQueryHandler<GetCartDetailsQuery, CartDetailsDto>
    {
        private readonly ICartManager _cartManager;

        public GetCartDetailsQueryHandler(ICartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public async Task<Result<CartDetailsDto>> Handle(
            GetCartDetailsQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _cartManager.GetActiveCartDetailsAsync(cancellationToken);
            return result;
        }
    }
}
