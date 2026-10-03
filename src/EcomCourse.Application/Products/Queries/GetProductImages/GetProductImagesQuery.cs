using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Queries.GetProductImages
{
    public record GetProductImagesQuery(Guid productId) : IQuery<List<ProductImageDto>>;
}
