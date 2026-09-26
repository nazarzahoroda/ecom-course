using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly EcomCourseDbContext _context;
        private readonly IUserContext _currentUserService;
        public CartService(EcomCourseDbContext context, IUserContext currentUserService)
        private readonly ICustomerStore _customerStore;
        private readonly TimeProvider _timeProvider;

        public CartService(
            EcomCourseDbContext context,
            IUserContext currentUserService,
            ICustomerStore customerStore,
            TimeProvider timeProvider)
        {
            _context = context;
            _currentUserService = currentUserService;
            _customerStore = customerStore;
            _timeProvider = timeProvider;
        }

        public async Task<Result<CartDetailsDto>> GetActiveCartDetailsAsync(
            CancellationToken cancellationToken
        )
        {
            var customerId = _currentUserService.CustomerId;

            var cart = await _context
                .Carts.AsNoTracking()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(
                    c => c.CustomerId == customerId && c.Status == CartStatus.Active,
                    cancellationToken
                );

            if (cart is null || cart.Items.Count == 0)
            {
                return Result.Success(
                    new CartDetailsDto(Guid.Empty, new List<CartItemDetailsDto>(), 0m)
                );
            }

            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _context
                .Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    UnitPrice = p.Price.Amount,
                    Currency = p.Price.Currency.ToString(),
                    Sku = p.SKU.Value,
                })
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var missingProductIds = productIds.Except(products.Keys).ToList();

            if (missingProductIds.Count > 0)
                return Result.Failure<CartDetailsDto>(ProductErrors.Unavailable);

            var itemsDto = cart
                .Items.Where(item => products.ContainsKey(item.ProductId))
                .Select(item =>
                {
                    var product = products[item.ProductId];
                    return new CartItemDetailsDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Name = product.Name,
                        Sku = product.Sku,
                        UnitPrice = product.UnitPrice,
                        Currency = product.Currency,
                        Quantity = item.Quantity,
                    };
                })
                .ToList();

            var totalAmount = itemsDto.Sum(i => i.UnitPrice * i.Quantity);

            return Result.Success(new CartDetailsDto(cart.Id, itemsDto, totalAmount));
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
                // Викликаємо фабрику замість публічного конструктора
                var cartResult = Cart.Create(customerId);

                if (cartResult.IsFailure)
                {
                    return Result.Failure(cartResult.Error);
                }

                cart = cartResult.Value!;
                _context.Carts.Add(cart);
            }

            var productExists = await _context.Products.AnyAsync(
                p => p.Id == dto.ProductId,
                cancellationToken
            );

            if (!productExists)
            {
                return Result.Failure(ProductErrors.NotFound(dto.ProductId));
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
