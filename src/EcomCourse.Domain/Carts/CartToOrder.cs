using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Carts
{
    public record CartCheckoutItem(
       Guid ProductId,
       int Quantity);

    public record CartCheckoutData(
    Guid CustomerId,
    IReadOnlyCollection<CartCheckoutItem> Items);
}
