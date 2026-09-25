using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Abstractions
{
    public interface ICartManager
    {
        public Task<Result> AddItemToCartAsync(
            AddItemToCartDto dto,
            CancellationToken cancellationToken
        );
        public Task<Result> UpdateCartItemQuantityAsync(
            UpdateCartItemQuantityDto dto,
            CancellationToken cancellationToken
        );
        public Task<Result<Guid>> RemoveItemFromCartAsync(
            Guid id,
            CancellationToken cancellationToken
        );
        public Task<Result<Guid>> CheckoutCart(CancellationToken cancellationToken);
        Task<Result<CartDetailsDto>> GetActiveCartDetailsAsync(CancellationToken cancellationToken);
    }
}
