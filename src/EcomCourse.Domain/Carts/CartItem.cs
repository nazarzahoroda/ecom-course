using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Carts
{
    public class CartItem : Entity<Guid>
    {
        public Guid CartId { get; private set; }
        public Cart Cart { get; private set; } = null!;

        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        private CartItem() : base(Guid.Empty) { }

        private CartItem(Guid productId, int quantity) : base(Guid.NewGuid())
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public static Result<CartItem> Create(Guid productId, int quantity)
        {
            if (quantity <= 0)
                return Result.Failure<CartItem>(CartErrors.InvalidQuantity);

            return Result.Success(new CartItem(productId, quantity));
        }
        public Result IncreaseQuantity(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure(CartErrors.InvalidQuantity);

            Quantity += quantity;

            return Result.Success();
        }
    }
}
