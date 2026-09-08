using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Carts
{
    public class Cart : Entity<Guid>
    {
        public Guid CustomerId { get; private set; }

        public CartStatus Status { get; private set; }

        private readonly List<CartItem> _items = new();

        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        private Cart()
            : base(Guid.Empty) { }

        public Cart(Guid id, Guid customerId)
            : base(id)
        {
            CustomerId = customerId;
            Status = CartStatus.Active;
        }

        public Result AddItem(Guid productId, int quantity)
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);
            var item = _items.FirstOrDefault(x => x.ProductId == productId);
            if (item is not null)
                return Result.Failure(CartErrors.ItemExists);

            var newItem = CartItem.Create(productId, quantity);

            if (newItem.IsFailure)
            {
                return newItem;
            }

            _items.Add(newItem.Value!);

            return Result.Success();
        }

        public Result UpdateItemQuantity(Guid productId, int quantity)
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);

            var item = _items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
                return Result.Failure(CartErrors.CartItemNotFound);

            return item.ChangeQuantity(quantity);
        }

        public Result RemoveItem(Guid productId)
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);

            var item = _items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
                return Result.Failure(CartErrors.CartItemNotFound);

            _items.Remove(item);

            return Result.Success();
        }

        private Result EnsureActive()
        {
            if (Status != CartStatus.Active)
                return Result.Failure(CartErrors.CartNotActive);

            return Result.Success();
        }

        public Result Checkout()
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);

            Status = CartStatus.CheckedOut;

            return Result.Success();
        }

        public Result Abandon()
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);

            Status = CartStatus.Abandoned;

            return Result.Success();
        }
    }
}
