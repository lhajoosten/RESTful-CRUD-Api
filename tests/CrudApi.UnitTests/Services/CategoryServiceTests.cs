using AutoMapper;
using CrudApi.Application.DTOs;
using CrudApi.Application.Services;
using CrudApi.Core.Common;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace CrudApi.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _categoryService = new CategoryService(
            _categoryRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" },
            new Category { Id = Guid.NewGuid(), Name = "Books", Slug = "books" }
        };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Id = categories[0].Id, Name = "Electronics", Slug = "electronics" },
            new CategoryDto { Id = categories[1].Id, Name = "Books", Slug = "books" }
        };

        _categoryRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(categories))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categoryDtos);
                _categoryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics", Slug = "electronics" };
        var categoryDto = new CategoryDto { Id = categoryId, Name = "Electronics", Slug = "electronics" };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mapperMock.Setup(x => x.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(categoryDto);
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Should().BeNull();
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory_WhenValidData()
    {
        // Arrange
        var createDto = new CreateCategoryDto 
        { 
            Name = "Electronics", 
            Description = "Electronic products",
            IsActive = true
        };
        var category = new Category 
        { 
            Id = Guid.NewGuid(), 
            Name = "Electronics", 
            Description = "Electronic products",
            Slug = "electronics"
        };
        var categoryDto = new CategoryDto 
        { 
            Id = category.Id, 
            Name = "Electronics", 
            Description = "Electronic products",
            Slug = "electronics"
        };

        _mapperMock.Setup(x => x.Map<Category>(createDto))
            .Returns(category);
        _categoryRepositoryMock.Setup(x => x.SlugExistsAsync("electronics", null))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(x => x.AddAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(category));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act
        var result = await _categoryService.CreateCategoryAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(categoryDto);
        _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrowException_WhenParentCategoryNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var createDto = new CreateCategoryDto 
        { 
            Name = "Smartphones", 
            ParentCategoryId = parentId
        };
        var category = new Category { Id = Guid.NewGuid(), Name = "Smartphones" };

        _mapperMock.Setup(x => x.Map<Category>(createDto))
            .Returns(category);
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _categoryService.CreateCategoryAsync(createDto));
        exception.Message.Should().Be("Parent category not found.");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenValidData()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto 
        { 
            Name = "Updated Electronics", 
            Description = "Updated description"
        };
        var existingCategory = new Category 
        { 
            Id = categoryId, 
            Name = "Electronics", 
            Slug = "electronics"
        };
        var updatedCategoryDto = new CategoryDto 
        { 
            Id = categoryId, 
            Name = "Updated Electronics", 
            Slug = "updated-electronics"
        };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);
        _mapperMock.Setup(x => x.Map(updateDto, existingCategory));
        _categoryRepositoryMock.Setup(x => x.SlugExistsAsync("updated-electronics", categoryId))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(existingCategory));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<CategoryDto>(existingCategory))
            .Returns(updatedCategoryDto);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedCategoryDto);
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnNull_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Name" };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);

        // Assert
        result.Should().BeNull();
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteCategory_WhenCategoryHasNoChildrenOrProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(x => x.GetByParentIdAsync(categoryId))
            .ReturnsAsync(new List<Category>());
        _productRepositoryMock.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        _categoryRepositoryMock.Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(category));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeFalse();
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldThrowException_WhenCategoryHasChildren()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var childCategories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Smartphones", ParentCategoryId = categoryId }
        };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(x => x.GetByParentIdAsync(categoryId))
            .ReturnsAsync(childCategories);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _categoryService.DeleteCategoryAsync(categoryId));
        exception.Message.Should().Be("Cannot delete category that has child categories.");
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldThrowException_WhenCategoryHasProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "iPhone", CategoryId = categoryId }
        };

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(x => x.GetByParentIdAsync(categoryId))
            .ReturnsAsync(new List<Category>());
        _productRepositoryMock.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _categoryService.DeleteCategoryAsync(categoryId));
        exception.Message.Should().Be("Cannot delete category that contains products.");
    }

    [Fact]
    public async Task GetCategoriesWithChildrenAsync_ShouldReturnHierarchicalCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" },
            new Category { Id = Guid.NewGuid(), Name = "Books", Slug = "books" }
        };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Id = categories[0].Id, Name = "Electronics", Slug = "electronics" },
            new CategoryDto { Id = categories[1].Id, Name = "Books", Slug = "books" }
        };

        _categoryRepositoryMock.Setup(x => x.GetCategoriesWithChildrenAsync())
            .ReturnsAsync(categories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(categories))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetCategoriesWithChildrenAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categoryDtos);
        _categoryRepositoryMock.Verify(x => x.GetCategoriesWithChildrenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnOnlyRootCategories()
    {
        // Arrange
        var rootCategories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics", ParentCategoryId = null },
            new Category { Id = Guid.NewGuid(), Name = "Books", Slug = "books", ParentCategoryId = null }
        };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Id = rootCategories[0].Id, Name = "Electronics", Slug = "electronics" },
            new CategoryDto { Id = rootCategories[1].Id, Name = "Books", Slug = "books" }
        };

        _categoryRepositoryMock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(rootCategories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(rootCategories))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetRootCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categoryDtos);
        _categoryRepositoryMock.Verify(x => x.GetRootCategoriesAsync(), Times.Once);
    }
}
