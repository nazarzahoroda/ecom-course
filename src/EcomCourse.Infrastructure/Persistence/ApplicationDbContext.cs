using EcomCourse.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable("Customers");

            builder.HasKey(customer => customer.Id);

            builder.Property(customer => customer.UserId)
                .IsRequired();

            builder.Property(customer => customer.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.OwnsOne(customer => customer.Email, emailBuilder =>
            {
                emailBuilder.Property(email => email.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(254)
                    .IsRequired();

                emailBuilder.HasIndex(email => email.Value)
                    .IsUnique();
            });

            builder.OwnsOne(customer => customer.Address, addressBuilder =>
            {
                addressBuilder.Property(address => address.Street)
                    .HasMaxLength(200)
                    .IsRequired();

                addressBuilder.Property(address => address.City)
                    .HasMaxLength(100)
                    .IsRequired();

                addressBuilder.Property(address => address.PostalCode)
                    .HasMaxLength(30)
                    .IsRequired();

                addressBuilder.Property(address => address.Country)
                    .HasMaxLength(100)
                    .IsRequired();
            });
        });
    }
}
