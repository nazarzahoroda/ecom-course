using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand
{
    public class RemoveItemFromCartCommandHandler : ICommandHandler<RemoveItemFromCartCommand>
    {
        private readonly IApplicationDbContext _context;

        public RemoveItemFromCartCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == request.id, cancellationToken);

            if (item is null)
            {
                return Result.Failure(CartErrors.NotFound);
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

    }
}
