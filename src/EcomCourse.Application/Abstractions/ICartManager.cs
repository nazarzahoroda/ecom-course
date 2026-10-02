using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Abstractions;

public interface ICartManager
{
    Task<Result> AddItemToCartAsync(
        AddItemToCartDto dto,
        CancellationToken cancellationToken
    );

    Task<Result> UpdateCartItemQuantityAsync(
        UpdateCartItemQuantityDto dto,
        CancellationToken cancellationToken
    );

    Task<Result<Guid>> RemoveItemFromCartAsync(
        Guid id,
        CancellationToken cancellationToken
    );

    Task<Result<CartDetailsDto>> GetActiveCartDetailsAsync(
        CancellationToken cancellationToken
    );
}
