using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CrudApi.Api.Models;
using CrudApi.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CrudApi.IntegrationTests.Controllers;

public class CategoriesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CategoriesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetCategories_ShouldReturnListOfCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CategoryDto>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetCategory_WithValidId_ShouldReturnCategory()
    {
        // Arrange - Use the seeded category ID
        var categoryId = new Guid("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(categoryId);
        apiResponse.Data.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/categories/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCategoryBySlug_WithValidSlug_ShouldReturnCategory()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/slug/electronics");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Slug.Should().Be("electronics");
        apiResponse.Data.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetCategoryBySlug_WithInvalidSlug_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/slug/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ShouldCreateCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "A test category for integration testing"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", createDto);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("Test Category");
        apiResponse.Data.Description.Should().Be("A test category for integration testing");
        apiResponse.Data.Slug.Should().Be("test-category");
        apiResponse.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCategory_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Empty name should be invalid
        var createDto = new CreateCategoryDto
        {
            Name = "", // Invalid - empty name
            Description = "A test category"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithValidData_ShouldUpdateCategory()
    {
        // Arrange - First create a category
        var createDto = new CreateCategoryDto
        {
            Name = "Category to Update",
            Description = "Original description"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var categoryId = createApiResponse!.Data!.Id;

        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Category Name",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", updateDto);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("Updated Category Name");
        apiResponse.Data.Description.Should().Be("Updated description");
        apiResponse.Data.Id.Should().Be(categoryId);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ShouldDeleteCategory()
    {
        // Arrange - First create a category
        var createDto = new CreateCategoryDto
        {
            Name = "Category to Delete",
            Description = "This category will be deleted"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var categoryId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the category is actually deleted
        var getResponse = await _client.GetAsync($"/api/categories/{categoryId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRootCategories_ShouldReturnCategoriesWithoutParent()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/root");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CategoryDto>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().OnlyContain(c => c.ParentCategoryId == null);
    }

    [Fact]
    public async Task GetCategoryProducts_ShouldReturnProductsInCategory()
    {
        // Arrange - Use the seeded category ID that has products
        var categoryId = new Guid("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}/products");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductDto>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCountGreaterThan(0);
        apiResponse.Data.Should().OnlyContain(p => p.CategoryId == categoryId);
    }
}
