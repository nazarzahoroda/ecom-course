namespace EcomCourse.Application.Carts.DTOs
{
    public record CartItemDetailsDto(
        Guid Id,
        Guid ProductId,
        string Name,
        string Sku,
        decimal UnitPrice,
        int Quantity
    );
}
