using AutoMapper;
using CrudApi.Api.Controllers;
using CrudApi.Api.Models;
using CrudApi.Application.DTOs;
using CrudApi.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CrudApi.UnitTests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CategoriesController>> _loggerMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _categoryServiceMock = new Mock<ICategoryService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CategoriesController>>();
        
        _controller = new CategoriesController(
            _categoryServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnAllCategories_WhenIncludeChildrenIsFalse()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics", IsActive = true },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Books", IsActive = true }
        };

        _categoryServiceMock.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(categories))
            .Returns(categories);

        // Act
        var result = await _controller.GetCategories(includeChildren: false, activeOnly: true);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<IEnumerable<CategoryDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnCategoriesWithChildren_WhenIncludeChildrenIsTrue()
    {
        // Arrange
        var categoriesWithChildren = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics", IsActive = true, ChildCategories = new List<CategoryDto>() },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Books", IsActive = true, ChildCategories = new List<CategoryDto>() }
        };

        _categoryServiceMock.Setup(x => x.GetCategoriesWithChildrenAsync())
            .ReturnsAsync(categoriesWithChildren);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(categoriesWithChildren))
            .Returns(categoriesWithChildren);

        // Act
        var result = await _controller.GetCategories(includeChildren: true, activeOnly: true);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        
        var apiResponse = actionResult!.Value as ApiResponse<IEnumerable<CategoryDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        _categoryServiceMock.Verify(x => x.GetCategoriesWithChildrenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCategory_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new CategoryDto { Id = categoryId, Name = "Electronics" };

        _categoryServiceMock.Setup(x => x.GetCategoryByIdAsync(categoryId))
            .ReturnsAsync(category);
        _mapperMock.Setup(x => x.Map<CategoryDto>(category))
            .Returns(category);

        // Act
        var result = await _controller.GetCategory(categoryId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<CategoryDto>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task GetCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryServiceMock.Setup(x => x.GetCategoryByIdAsync(categoryId))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _controller.GetCategory(categoryId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as NotFoundObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(404);
        
        var apiResponse = actionResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Category not found");
    }

    [Fact]
    public async Task GetCategoryBySlug_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var slug = "electronics";
        var category = new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics", Slug = slug };

        _categoryServiceMock.Setup(x => x.GetCategoryBySlugAsync(slug))
            .ReturnsAsync(category);
        _mapperMock.Setup(x => x.Map<CategoryDto>(category))
            .Returns(category);

        // Act
        var result = await _controller.GetCategoryBySlug(slug);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<CategoryDto>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreatedCategory_WhenValidData()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Electronics", Description = "Electronic products" };
        var createdCategory = new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic products" };

        _categoryServiceMock.Setup(x => x.CreateCategoryAsync(createDto))
            .ReturnsAsync(createdCategory);
        _mapperMock.Setup(x => x.Map<CategoryDto>(createdCategory))
            .Returns(createdCategory);

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as CreatedAtActionResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(201);
        
        var apiResponse = actionResult.Value as ApiResponse<CategoryDto>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(createdCategory);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnUpdatedCategory_WhenValidData()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Electronics" };
        var updatedCategory = new CategoryDto { Id = categoryId, Name = "Updated Electronics" };

        _categoryServiceMock.Setup(x => x.UpdateCategoryAsync(categoryId, updateDto))
            .ReturnsAsync(updatedCategory);
        _mapperMock.Setup(x => x.Map<CategoryDto>(updatedCategory))
            .Returns(updatedCategory);

        // Act
        var result = await _controller.UpdateCategory(categoryId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<CategoryDto>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(updatedCategory);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Electronics" };

        _categoryServiceMock.Setup(x => x.UpdateCategoryAsync(categoryId, updateDto))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _controller.UpdateCategory(categoryId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as NotFoundObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(404);
        
        var apiResponse = actionResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Category not found");
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnSuccess_WhenCategoryDeleted()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryServiceMock.Setup(x => x.DeleteCategoryAsync(categoryId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Category deleted successfully");
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryServiceMock.Setup(x => x.DeleteCategoryAsync(categoryId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as NotFoundObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(404);
        
        var apiResponse = actionResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Category not found");
    }

    [Fact]
    public async Task GetRootCategories_ShouldReturnRootCategories()
    {
        // Arrange
        var rootCategories = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics", ParentCategoryId = null },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Books", ParentCategoryId = null }
        };

        _categoryServiceMock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(rootCategories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(rootCategories))
            .Returns(rootCategories);

        // Act
        var result = await _controller.GetRootCategories();

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<IEnumerable<CategoryDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(c => c.ParentCategoryId == null);
    }

    [Fact]
    public async Task GetChildCategories_ShouldReturnChildCategories()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childCategories = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Smartphones", ParentCategoryId = parentId },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Laptops", ParentCategoryId = parentId }
        };

        _categoryServiceMock.Setup(x => x.GetChildCategoriesAsync(parentId))
            .ReturnsAsync(childCategories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(childCategories))
            .Returns(childCategories);

        // Act
        var result = await _controller.GetChildCategories(parentId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<IEnumerable<CategoryDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(c => c.ParentCategoryId == parentId);
    }

    [Fact]
    public async Task GetCategoryProducts_ShouldReturnProductsInCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "iPhone 15", CategoryId = categoryId },
            new ProductDto { Id = Guid.NewGuid(), Name = "Samsung Galaxy", CategoryId = categoryId }
        };

        _categoryServiceMock.Setup(x => x.GetCategoryProductsAsync(categoryId))
            .ReturnsAsync(products);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(products);

        // Act
        var result = await _controller.GetCategoryProducts(categoryId);

        // Assert
        result.Should().NotBeNull();
        var actionResult = result.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().Be(200);
        
        var apiResponse = actionResult.Value as ApiResponse<IEnumerable<ProductDto>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().OnlyContain(p => p.CategoryId == categoryId);
    }
}
