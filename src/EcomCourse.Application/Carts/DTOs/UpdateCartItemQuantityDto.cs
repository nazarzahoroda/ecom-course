namespace EcomCourse.Application.Carts.DTOs
{
    public class UpdateCartItemQuantityDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
