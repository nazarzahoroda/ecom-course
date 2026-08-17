using EcomCourse.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomCourse.Infrastructure.Persistence.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(ol => ol.Id);

        builder.Property(ol => ol.ProductId)
            .IsRequired();

        builder.Property(ol => ol.Quantity)
            .IsRequired();

        builder.Property(ol => ol.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}