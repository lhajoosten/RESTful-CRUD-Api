using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CrudApi.Api.Models;
using CrudApi.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CrudApi.IntegrationTests.Controllers;

public class ProductsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProductsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetProducts_ShouldReturnListOfProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
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
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange - Use the seeded product ID
        var productId = new Guid("33333333-3333-3333-3333-333333333333");

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(productId);
        apiResponse.Data.Name.Should().Be("iPhone 15");
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "A test product for integration testing",
            Price = 99.99m,
            CategoryId = new Guid("11111111-1111-1111-1111-111111111111") // Electronics category
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createDto);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("Test Product");
        apiResponse.Data.Description.Should().Be("A test product for integration testing");
        apiResponse.Data.Price.Should().Be(99.99m);
        apiResponse.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Empty name and negative price should be invalid
        var createDto = new CreateProductDto
        {
            Name = "", // Invalid - empty name
            Description = "A test product",
            Price = -10m // Invalid - negative price
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProduct()
    {
        // Arrange - First create a product
        var createDto = new CreateProductDto
        {
            Name = "Product to Update",
            Description = "Original description",
            Price = 50.00m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var productId = createApiResponse!.Data!.Id;

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product Name",
            Description = "Updated description",
            Price = 75.00m
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateDto);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("Updated Product Name");
        apiResponse.Data.Description.Should().Be("Updated description");
        apiResponse.Data.Price.Should().Be(75.00m);
        apiResponse.Data.Id.Should().Be(productId);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldDeleteProduct()
    {
        // Arrange - First create a product
        var createDto = new CreateProductDto
        {
            Name = "Product to Delete",
            Description = "This product will be deleted",
            Price = 25.00m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var productId = createApiResponse!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the product is actually deleted
        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchProducts_WithValidSearchTerm_ShouldReturnMatchingProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=iPhone");
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
        apiResponse.Data.Should().OnlyContain(p => p.Name.Contains("iPhone") || p.Description.Contains("iPhone"));
    }

    [Fact]
    public async Task GetProductsByPriceRange_ShouldReturnProductsInRange()
    {
        // Act
        var response = await _client.GetAsync("/api/products/price-range?minPrice=10&maxPrice=20");
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
        // Should contain "The Great Gatsby" which is priced at 15.99
        apiResponse.Data.Should().OnlyContain(p => p.Price >= 10 && p.Price <= 20);
    }

    [Fact]
    public async Task GetProductsByCategory_ShouldReturnProductsInCategory()
    {
        // Arrange - Use the Electronics category ID
        var categoryId = new Guid("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/products/category/{categoryId}");
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

    [Fact]
    public async Task GetProductsByCategory_WithInvalidCategoryId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidCategoryId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/category/{invalidCategoryId}");
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
        apiResponse.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductsByPriceRange_WithInvalidRange_ShouldReturnBadRequest()
    {
        // Act - Min price greater than max price should be invalid
        var response = await _client.GetAsync("/api/products/price-range?minPrice=100&maxPrice=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
