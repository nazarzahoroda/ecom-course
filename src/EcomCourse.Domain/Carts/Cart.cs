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


        private Cart() : base(Guid.Empty) { }

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

            var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem is not null)
            {
                return existingItem.IncreaseQuantity(quantity);
            }

            var newItem = CartItem.Create(productId, quantity);

            if (newItem.IsFailure)
            {
                return newItem;
            }

            _items.Add(newItem.Value!);

            return Result.Success();
        }

        public Result RemoveItem(Guid productId)
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure(isActive.Error);

            var item = _items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
                return Result.Failure(CartErrors.NotFound);

            _items.Remove(item);

            return Result.Success();
        }

        public Result<CartCheckoutData> Checkout()
        {
            var isActive = EnsureActive();

            if (isActive.IsFailure)
                return Result.Failure<CartCheckoutData>(isActive.Error);

            if (_items.Count == 0)
                return Result.Failure<CartCheckoutData>(CartErrors.NotFound);

            Status = CartStatus.CheckedOut;

            var data = new CartCheckoutData
              (CustomerId, _items.Select(x => new CartCheckoutItem(x.ProductId, x.Quantity)).ToList());

            return Result.Success(data);
        }

        private Result EnsureActive()
        {
            if (Status != CartStatus.Active)
                return Result.Failure(CartErrors.CartNotActive);

            return Result.Success();
        }
    }
}
