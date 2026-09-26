using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Products
{
    public class ProductImage : Entity<Guid>
    {
        public Guid ProductId { get; private set; }
        public string BlobName { get; private set; } = string.Empty;
        public string ContentType { get; private set; } = string.Empty;
        public bool IsMain { get; private set; }

        private ProductImage()
            : base(Guid.Empty) { }

        public static ProductImage Create(
            Guid productId,
            string blobName,
            string contentType,
            bool isMain = false
        )
        {
            return new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                BlobName = blobName,
                ContentType = contentType,
                IsMain = isMain,
            };
        }

        public void SetMain(bool isMain) => IsMain = isMain;
    }
}
