using AutoMapper;
using CrudApi.Application.DTOs;
using CrudApi.Application.Services;
using CrudApi.Core.Common;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace CrudApi.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _productService = new ProductService(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "iPhone 15", Price = 999.99m, StockQuantity = 50 },
            new Product { Id = Guid.NewGuid(), Name = "Samsung Galaxy", Price = 899.99m, StockQuantity = 30 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto { Id = products[0].Id, Name = "iPhone 15", Price = 999.99m, StockQuantity = 50 },
            new ProductDto { Id = products[1].Id, Name = "Samsung Galaxy", Price = 899.99m, StockQuantity = 30 }
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(productDtos);
        _productRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "iPhone 15", Price = 999.99m };
        var productDto = new ProductDto { Id = productId, Name = "iPhone 15", Price = 999.99m };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct_WhenValidData()
    {
        // Arrange
        var createDto = new CreateProductDto 
        { 
            Name = "iPhone 15", 
            Description = "Latest iPhone model",
            Price = 999.99m,
            StockQuantity = 50
        };
        var product = new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = "iPhone 15", 
            Description = "Latest iPhone model",
            Price = 999.99m,
            StockQuantity = 50
        };
        var productDto = new ProductDto 
        { 
            Id = product.Id, 
            Name = "iPhone 15", 
            Description = "Latest iPhone model",
            Price = 999.99m,
            StockQuantity = 50
        };

        _mapperMock.Setup(x => x.Map<Product>(createDto))
            .Returns(product);
        _productRepositoryMock.Setup(x => x.AddAsync(product, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(product));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto 
        { 
            Name = "Updated iPhone", 
            Price = 1099.99m,
            StockQuantity = 25
        };
        var existingProduct = new Product 
        { 
            Id = productId, 
            Name = "iPhone 15", 
            Price = 999.99m,
            StockQuantity = 50
        };
        var updatedProductDto = new ProductDto 
        { 
            Id = productId, 
            Name = "Updated iPhone", 
            Price = 1099.99m,
            StockQuantity = 25
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mapperMock.Setup(x => x.Map(updateDto, existingProduct));
        _productRepositoryMock.Setup(x => x.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(existingProduct));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<ProductDto>(existingProduct))
            .Returns(updatedProductDto);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProductDto);
        _productRepositoryMock.Verify(x => x.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnNull_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "Updated Name" };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateDto);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "iPhone 15" };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepositoryMock.Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(product));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeTrue();
        _productRepositoryMock.Verify(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeFalse();
        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var category = "Electronics";
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "iPhone 15", CategoryId = categoryId },
            new Product { Id = Guid.NewGuid(), Name = "Samsung Galaxy", CategoryId = categoryId }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto { Id = products[0].Id, Name = "iPhone 15", CategoryId = categoryId },
            new ProductDto { Id = products[1].Id, Name = "Samsung Galaxy", CategoryId = categoryId }
        };

        _productRepositoryMock.Setup(x => x.GetByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetProductsByCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(productDtos);
        _productRepositoryMock.Verify(x => x.GetByCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var activeProducts = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "iPhone 15", IsActive = true },
            new Product { Id = Guid.NewGuid(), Name = "Samsung Galaxy", IsActive = true }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto { Id = activeProducts[0].Id, Name = "iPhone 15", IsActive = true },
            new ProductDto { Id = activeProducts[1].Id, Name = "Samsung Galaxy", IsActive = true }
        };

        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeProducts);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(activeProducts))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetActiveProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
        _productRepositoryMock.Verify(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ShouldReturnLowStockProducts()
    {
        // Arrange
        var threshold = 10;
        var lowStockProducts = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "iPhone 15", StockQuantity = 5 },
            new Product { Id = Guid.NewGuid(), Name = "Samsung Galaxy", StockQuantity = 2 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto { Id = lowStockProducts[0].Id, Name = "iPhone 15", StockQuantity = 5 },
            new ProductDto { Id = lowStockProducts[1].Id, Name = "Samsung Galaxy", StockQuantity = 2 }
        };

        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowStockProducts);
        _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(lowStockProducts))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetLowStockProductsAsync(threshold);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.StockQuantity <= threshold);
        _productRepositoryMock.Verify(x => x.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()), Times.Once);
    }
}
