namespace EcomCourse.Application.Products
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? Url { get; set; }
        public bool IsMain { get; set; }
    }
}
