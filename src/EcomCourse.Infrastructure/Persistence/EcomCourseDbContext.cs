using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

public class EcomCourseDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public EcomCourseDbContext(DbContextOptions<EcomCourseDbContext> options)
        : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .ToTable("AspNetUsers", "identity");

        builder.Entity<IdentityRole<Guid>>()
            .ToTable("AspNetRoles", "identity");

        builder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("AspNetRoleClaims", "identity");

        builder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("AspNetUserClaims", "identity");

        builder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("AspNetUserLogins", "identity");

        builder.Entity<IdentityUserRole<Guid>>()
            .ToTable("AspNetUserRoles", "identity");

        builder.Entity<IdentityUserToken<Guid>>()
            .ToTable("AspNetUserTokens", "identity");

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "identity");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.ReplacedByTokenHash)
                .HasMaxLength(64);

            entity
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();
        });

        builder.ApplyConfigurationsFromAssembly(
            typeof(EcomCourseDbContext).Assembly);
    }
}
