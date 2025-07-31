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

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        
        _controller = new ProductsController(
            _productServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "iPhone 15", Price = 999.99m },
            new ProductDto { Id = Guid.NewGuid(), Name = "Samsung Galaxy", Price = 799.99m }
        };

        _productServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProducts(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        resultProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "iPhone 15", Price = 999.99m };

        _productServiceMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var returnedProduct = okResult.Value as ProductDto;
        returnedProduct.Should().NotBeNull();
        returnedProduct.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productServiceMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedProduct_WhenValidData()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "iPhone 15", Price = 999.99m, Description = "Latest iPhone" };
        var createdProduct = new ProductDto { Id = Guid.NewGuid(), Name = "iPhone 15", Price = 999.99m, Description = "Latest iPhone" };

        _productServiceMock.Setup(x => x.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        
        var returnedProduct = createdResult.Value as ProductDto;
        returnedProduct.Should().NotBeNull();
        returnedProduct.Should().BeEquivalentTo(createdProduct);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnUpdatedProduct_WhenValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "iPhone 15 Pro", Price = 1199.99m };
        var updatedProduct = new ProductDto { Id = productId, Name = "iPhone 15 Pro", Price = 1199.99m };

        _productServiceMock.Setup(x => x.UpdateAsync(productId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);
        _mapperMock.Setup(x => x.Map<ProductDto>(updatedProduct))
            .Returns(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultProduct = okResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
        resultProduct.Should().BeEquivalentTo(updatedProduct);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "iPhone 15 Pro" };

        _productServiceMock.Setup(x => x.UpdateAsync(productId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.UpdateProduct(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeAssignableTo<string>();
        notFoundResult.Value.ToString().Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnSuccess_WhenProductDeleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productServiceMock.Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productServiceMock.Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeAssignableTo<string>();
        notFoundResult.Value.ToString().Should().Contain("not found");
    }

    [Fact]
    public async Task SearchProducts_ShouldReturnFilteredProducts_WhenSearchTermProvided()
    {
        // Arrange
        var searchTerm = "iPhone";
        var filteredProducts = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "iPhone 15", Price = 999.99m },
            new ProductDto { Id = Guid.NewGuid(), Name = "iPhone 14", Price = 799.99m }
        };

        _productServiceMock.Setup(x => x.SearchProductsAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredProducts);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(filteredProducts))
            .Returns(filteredProducts);

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        resultProducts.Should().HaveCount(2);
        resultProducts.Should().OnlyContain(p => p.Name.Contains("iPhone"));
    }

    [Fact]
    public async Task GetProductsByPriceRange_ShouldReturnProductsInRange()
    {
        // Arrange
        var minPrice = 500m;
        var maxPrice = 1000m;
        var productsInRange = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "Product 1", Price = 600m },
            new ProductDto { Id = Guid.NewGuid(), Name = "Product 2", Price = 800m }
        };

        _productServiceMock.Setup(x => x.GetProductsByPriceRangeAsync(minPrice, maxPrice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productsInRange);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(productsInRange))
            .Returns(productsInRange);

        // Act
        var result = await _controller.GetProductsByPriceRange(minPrice, maxPrice);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        resultProducts.Should().HaveCount(2);
        resultProducts.Should().OnlyContain(p => p.Price >= minPrice && p.Price <= maxPrice);
    }

    [Fact]
    public async Task GetProductsByCategory_ShouldReturnProductsInCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var productsInCategory = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "Product 1", CategoryId = categoryId },
            new ProductDto { Id = Guid.NewGuid(), Name = "Product 2", CategoryId = categoryId }
        };

        _productServiceMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productsInCategory);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(productsInCategory))
            .Returns(productsInCategory);

        // Act
        var result = await _controller.GetProductsByCategoryId(categoryId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        resultProducts.Should().HaveCount(2);
        resultProducts.Should().OnlyContain(p => p.CategoryId == categoryId);
    }
}
