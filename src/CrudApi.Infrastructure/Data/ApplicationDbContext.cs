using Microsoft.EntityFrameworkCore;
using CrudApi.Core.Entities;

namespace CrudApi.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop Pro",
                Description = "High-performance laptop for professionals",
                Price = 1299.99m,
                Category = "Electronics",
                StockQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 29.99m,
                Category = "Electronics",
                StockQuantity = 150,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Product
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Office Chair",
                Description = "Comfortable ergonomic office chair",
                Price = 249.99m,
                Category = "Furniture",
                StockQuantity = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}
