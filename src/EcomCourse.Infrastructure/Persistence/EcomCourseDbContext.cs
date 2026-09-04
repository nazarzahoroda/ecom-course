using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Orders;
using EcomCourse.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.Infrastructure.Persistence;

    public class EcomCourseDbContext : DbContext, IApplicationDbContext
{
        public EcomCourseDbContext(DbContextOptions<EcomCourseDbContext> options)
            : base(options)
        {
        }
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcomCourseDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
