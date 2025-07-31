using CrudApi.Core.Entities;
using CrudApi.Infrastructure.Data;
using CrudApi.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CrudApi.UnitTests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetByPriceRangeAsync_ShouldReturnProductsInRange()
    {
        // Arrange
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Cheap Product",
            Price = 100m,
            Description = "A cheap product",
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Expensive Product",
            Price = 1000m,
            Description = "An expensive product",
            CreatedAt = DateTime.UtcNow
        };

        var product3 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Mid Range Product",
            Price = 500m,
            Description = "A mid-range product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPriceRangeAsync(200m, 800m);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Mid Range Product");
        result.First().Price.Should().Be(500m);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product in Category",
            Price = 100m,
            CategoryId = categoryId,
            Description = "Product in specific category",
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product in Other Category",
            Price = 200m,
            CategoryId = otherCategoryId,
            Description = "Product in other category",
            CreatedAt = DateTime.UtcNow
        };

        var product3 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product Without Category",
            Price = 300m,
            Description = "Product without category",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryIdAsync(categoryId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Product in Category");
        result.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingProducts()
    {
        // Arrange
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "iPhone 15",
            Price = 999m,
            Description = "Latest Apple smartphone",
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Samsung Galaxy",
            Price = 799m,
            Description = "Android smartphone",
            CreatedAt = DateTime.UtcNow
        };

        var product3 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "iPad Pro",
            Price = 1099m,
            Description = "Apple tablet device",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("iPhone");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("iPhone 15");
    }

    [Fact]
    public async Task SearchAsync_ShouldSearchInDescription()
    {
        // Arrange
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Device A",
            Price = 999m,
            Description = "This is an Apple product",
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Device B",
            Price = 799m,
            Description = "This is a Samsung product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Apple");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Device A");
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var activeProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Active Product",
            Price = 100m,
            IsActive = true,
            Description = "An active product",
            CreatedAt = DateTime.UtcNow
        };

        var inactiveProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Product",
            Price = 200m,
            IsActive = false,
            Description = "An inactive product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddRangeAsync(activeProduct, inactiveProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(p => p.IsActive);
        result.First().Name.Should().Be("Active Product");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 100m,
            Description = "A test product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(productId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetWithCategoryAsync_ShouldIncludeCategoryInformation()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 100m,
            CategoryId = category.Id,
            Description = "A test product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithCategoryAsync();

        // Assert
        result.Should().HaveCount(1);
        var productWithCategory = result.First();
        productWithCategory.ProductCategory.Should().NotBeNull();
        productWithCategory.ProductCategory!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetAllWithCategoriesAsync_ShouldIncludeCategoryInformation()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 100m,
            CategoryId = category.Id,
            Description = "A test product",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductCategory.Should().NotBeNull();
        result.First().ProductCategory!.Name.Should().Be("Electronics");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
