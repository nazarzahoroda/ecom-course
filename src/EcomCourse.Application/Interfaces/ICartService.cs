using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Interfaces
{
    public interface ICartService
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
    }
}
