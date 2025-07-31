using Microsoft.EntityFrameworkCore;
using CrudApi.Core.Entities;

namespace CrudApi.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProduct(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureProductTag(modelBuilder);
        ConfigureProductImage(modelBuilder);

        // Seed data
        SeedData(modelBuilder);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasMaxLength(100); // Legacy field
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Sku).HasMaxLength(50);
            entity.Property(e => e.Weight).HasPrecision(10, 3);
            entity.Property(e => e.Dimensions).HasMaxLength(200);
            entity.Property(e => e.MetaData).HasColumnType("nvarchar(max)");

            // Relationships
            entity.HasOne(e => e.ProductCategory)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Tags)
                  .WithMany(t => t.Products)
                  .UsingEntity(j => j.ToTable("ProductProductTags"));

            entity.HasMany(e => e.Images)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.StockQuantity);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MetaData).HasColumnType("nvarchar(max)");

            // Self-referencing relationship for hierarchy
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(c => c.ChildCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.ParentCategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.DisplayOrder);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureProductTag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7); // Hex color code

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsDeleted);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureProductImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.MimeType).HasMaxLength(50);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.IsPrimary);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.DisplayOrder);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        var electronicsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var computersId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var peripheralsId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var furnitureId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var categories = new[]
        {
            new Category
            {
                Id = electronicsId,
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                Slug = "electronics",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            new Category
            {
                Id = computersId,
                Name = "Computers",
                Description = "Laptops, desktops, and computer accessories",
                Slug = "computers",
                ParentCategoryId = electronicsId,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new Category
            {
                Id = peripheralsId,
                Name = "Peripherals",
                Description = "Computer peripherals and accessories",
                Slug = "peripherals",
                ParentCategoryId = electronicsId,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new Category
            {
                Id = furnitureId,
                Name = "Furniture",
                Description = "Office and home furniture",
                Slug = "furniture",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            }
        };

        // Seed Tags
        var tagIds = new[]
        {
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Guid.Parse("88888888-8888-8888-8888-888888888888")
        };

        var tags = new[]
        {
            new ProductTag { Id = tagIds[0], Name = "New", Color = "#28a745", CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new ProductTag { Id = tagIds[1], Name = "Popular", Color = "#ffc107", CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new ProductTag { Id = tagIds[2], Name = "Sale", Color = "#dc3545", CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new ProductTag { Id = tagIds[3], Name = "Premium", Color = "#6f42c1", CreatedAt = DateTime.UtcNow.AddDays(-40) }
        };

        // Seed Products with updated structure
        var products = new[]
        {
            new Product
            {
                Id = Guid.Parse("aa111111-1111-1111-1111-111111111111"),
                Name = "Laptop Pro",
                Description = "High-performance laptop for professionals",
                Price = 1299.99m,
                Category = "Electronics", // Legacy field
                CategoryId = computersId, // Computer category
                Sku = "LAP-PRO-001",
                StockQuantity = 50,
                MinStockLevel = 10,
                Weight = 1800.0m, // grams
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = Guid.Parse("bb222222-2222-2222-2222-222222222222"),
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 29.99m,
                Category = "Electronics", // Legacy field
                CategoryId = peripheralsId, // Peripherals category
                Sku = "MOU-WIR-001",
                StockQuantity = 150,
                MinStockLevel = 20,
                Weight = 85.0m, // grams
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Product
            {
                Id = Guid.Parse("cc333333-3333-3333-3333-333333333333"),
                Name = "Office Chair",
                Description = "Comfortable ergonomic office chair",
                Price = 249.99m,
                Category = "Furniture", // Legacy field
                CategoryId = furnitureId, // Furniture category
                Sku = "CHR-OFF-001",
                StockQuantity = 25,
                MinStockLevel = 5,
                Weight = 15000.0m, // grams
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        modelBuilder.Entity<Category>().HasData(categories);
        modelBuilder.Entity<ProductTag>().HasData(tags);
        modelBuilder.Entity<Product>().HasData(products);
    }
}
