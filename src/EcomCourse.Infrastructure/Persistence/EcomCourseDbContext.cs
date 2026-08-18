using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

public class EcomCourseDbContext : DbContext
{
    public EcomCourseDbContext(DbContextOptions<EcomCourseDbContext> options)
        : base(options)
    {
    }
    DbSet<Cart> Carts { get; set; }
    DbSet<CartItem> CartItems { get; set; }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcomCourseDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
