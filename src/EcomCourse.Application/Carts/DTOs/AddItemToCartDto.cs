using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Application.Carts.DTOs
{
    public class AddItemToCartDto
    {
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
    }
}
