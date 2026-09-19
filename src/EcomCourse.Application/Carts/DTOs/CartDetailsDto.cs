namespace EcomCourse.Application.Carts.DTOs
{
    public record CartDetailsDto(Guid CartId, List<CartItemDetailsDto> Items, decimal TotalAmount);
}
