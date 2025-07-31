using CrudApi.Core.Entities;
using CrudApi.Infrastructure.Data;
using CrudApi.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CrudApi.UnitTests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCategory_WhenSlugExists()
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

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySlugAsync("electronics");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("electronics");
        result.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugDoesNotExist()
    {
        // Act
        var result = await _repository.GetBySlugAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnCategoriesWithoutParent()
    {
        // Arrange
        var rootCategory1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rootCategory2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Books",
            Slug = "books",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var childCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Slug = "smartphones",
            ParentCategoryId = rootCategory1.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddRangeAsync(rootCategory1, rootCategory2, childCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRootCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.ParentCategoryId == null);
        result.Should().Contain(c => c.Name == "Electronics");
        result.Should().Contain(c => c.Name == "Books");
    }

    [Fact]
    public async Task GetChildCategoriesAsync_ShouldReturnChildCategories()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var parentCategory = new Category
        {
            Id = parentCategoryId,
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var childCategory1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Slug = "smartphones",
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var childCategory2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Laptops",
            Slug = "laptops",
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddRangeAsync(parentCategory, childCategory1, childCategory2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetChildCategoriesAsync(parentCategoryId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.ParentCategoryId == parentCategoryId);
        result.Should().Contain(c => c.Name == "Smartphones");
        result.Should().Contain(c => c.Name == "Laptops");
    }

    [Fact]
    public async Task GetCategoriesWithChildrenAsync_ShouldReturnCategoriesWithChildRelationships()
    {
        // Arrange
        var parentCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var childCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Slug = "smartphones",
            ParentCategoryId = parentCategory.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddRangeAsync(parentCategory, childCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCategoriesWithChildrenAsync();

        // Assert
        result.Should().HaveCount(2);
        var parent = result.First(c => c.Name == "Electronics");
        parent.ChildCategories.Should().HaveCount(1);
        parent.ChildCategories.First().Name.Should().Be("Smartphones");
    }

    [Fact]
    public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var activeCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Obsolete",
            Slug = "obsolete",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddRangeAsync(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(c => c.IsActive);
        result.First().Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(categoryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlugUniqueAsync_ShouldReturnTrue_WhenSlugIsUnique()
    {
        // Act
        var result = await _repository.IsSlugUniqueAsync("unique-slug");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlugUniqueAsync_ShouldReturnFalse_WhenSlugExists()
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

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IsSlugUniqueAsync("electronics");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlugUniqueAsync_ShouldReturnTrue_WhenSlugExistsButDifferentId()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Electronics",
            Slug = "electronics",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act - Check if slug is unique for a different category ID
        var result = await _repository.IsSlugUniqueAsync("electronics", categoryId);

        // Assert
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
