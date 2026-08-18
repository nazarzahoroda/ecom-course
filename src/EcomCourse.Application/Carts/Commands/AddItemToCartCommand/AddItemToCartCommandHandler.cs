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
            this._context = context;
        }

        public async Task<Result> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.SingleOrDefaultAsync(c => c.Id == request.dto.CartId, cancellationToken);
            if (cart is null)
            {
                return Result.Failure(CartErrors.NotFound);
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
