using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Carts.Commands.AddItemToCartCommand
{
    public class AddItemToCartCommandHandler : ICommandHandler<AddItemToCartCommand>
    {
        private readonly IApplicationDbContext _context;

        public AddItemToCartCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.Include(c => c.Items).SingleOrDefaultAsync(
            c => c.CustomerId == request.customerId &&
                 c.Status == CartStatus.Active,
            cancellationToken);
            if (cart is null)
            {
                cart = new Cart(
                    Guid.NewGuid(),
                    request.customerId);

                _context.Carts.Add(cart);
            }
            var result = cart.AddItem(
            request.dto.ProductId,
            request.dto.Quantity);

            if (result.IsFailure)
            {
                return result;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
