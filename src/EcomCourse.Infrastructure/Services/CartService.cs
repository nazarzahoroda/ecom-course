using Azure.Core;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly EcomCourseDbContext _context;
        private readonly IUserContext _currentUserService;

        public CartService(EcomCourseDbContext context, IUserContext currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> AddItemToCartAsync(
            AddItemToCartDto dto,
            CancellationToken cancellationToken
        )
        {
            var customerId = _currentUserService.CustomerId;

            var activeCarts = await _context
                .Carts.Include(c => c.Items)
                .Where(c => c.CustomerId == customerId && c.Status == CartStatus.Active)
                .Take(2)
                .ToListAsync(cancellationToken);
            if (activeCarts.Count > 1)
                return Result.Failure(CartErrors.ActiveCartAlreadyExists);

            var cart = activeCarts.FirstOrDefault();

            if (cart is null)
            {
                cart = new Cart(Guid.NewGuid(), customerId);

                _context.Carts.Add(cart);
            }
            var result = cart.AddItem(dto.ProductId, dto.Quantity);

            if (result.IsFailure)
            {
                return result;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> UpdateCartItemQuantityAsync(
            UpdateCartItemQuantityDto dto,
            CancellationToken cancellationToken
        )
        {
            var customerId = _currentUserService.CustomerId;

            var cart = await _context
                .Carts.Include(c => c.Items)
                .SingleOrDefaultAsync(
                    c => c.CustomerId == customerId && c.Status == CartStatus.Active,
                    cancellationToken
                );
            if (cart is null)
            {
                return Result.Failure(CartErrors.CartNotFound);
            }
            var result = cart.UpdateItemQuantity(dto.ProductId, dto.Quantity);

            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<Guid>> RemoveItemFromCartAsync(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var customerId = _currentUserService.CustomerId;

            var item = await _context.CartItems.FirstOrDefaultAsync(
                x => x.Id == id && x.Cart.CustomerId == customerId,
                cancellationToken
            );

            if (item is null)
            {
                return Result.Failure<Guid>(CartErrors.CartItemNotFound);
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(item.Id);
        }
    }
}
