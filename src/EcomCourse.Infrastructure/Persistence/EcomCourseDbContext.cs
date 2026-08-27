using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

public class EcomCourseDbContext
    : DbContext
{
    public EcomCourseDbContext(
        DbContextOptions<EcomCourseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EcomCourseDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
