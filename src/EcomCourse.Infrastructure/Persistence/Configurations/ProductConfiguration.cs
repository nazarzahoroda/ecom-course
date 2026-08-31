using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomCourse.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(product => product.Id);

            builder.Property(product => product.Name)
            .HasMaxLength(100)
            .IsRequired();

            builder.OwnsOne(product => product.Price, priceBuilder =>
            {
                priceBuilder.Property(price => price.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                priceBuilder.Property(price => price.Currency)
                    .HasConversion<string>()
                    .HasColumnName("Currency")
                    .IsRequired();
            });

            builder.OwnsOne(product => product.SKU, skuBuilder =>
            {
                skuBuilder.Property(sku => sku.Value)
                    .HasColumnName("SKUValue")
                    .IsRequired();
            });

            builder.Property(product => product.CategoryId)
                .IsRequired();

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(product => product.CategoryId);
        }
    }
}
