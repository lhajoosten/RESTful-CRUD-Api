using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using CrudApi.Application.DTOs;
using CrudApi.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Products API Controller - Manages product CRUD operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("GlobalPolicy")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all products</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all products");
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get active products only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active products</returns>
    /// <response code="200">Returns the list of active products</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActiveProducts(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active products");
        var products = await _productService.GetActiveProductsAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Product category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products in the specified category</returns>
    /// <response code="200">Returns the list of products in the category</response>
    /// <response code="400">If the category parameter is invalid</response>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
        [Required] string category, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Category cannot be empty");

        _logger.LogInformation("Getting products by category: {Category}", category);
        var products = await _productService.GetByCategoryAsync(category, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products with low stock</returns>
    /// <response code="200">Returns the list of products with low stock</response>
    /// <response code="400">If the threshold is invalid</response>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10, 
        CancellationToken cancellationToken = default)
    {
        if (threshold < 0)
            return BadRequest("Threshold must be non-negative");

        _logger.LogInformation("Getting products with low stock (threshold: {Threshold})", threshold);
        var products = await _productService.GetLowStockProductsAsync(threshold, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">If product is not found</response>
    /// <response code="400">If the ID format is invalid</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid product ID");

        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">Product creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <response code="201">Returns the newly created product</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto createProductDto, 
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);
        var product = await _productService.CreateAsync(createProductDto, cancellationToken);
        
        _logger.LogInformation("Created product with ID: {ProductId}", product.Id);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateProductDto">Product update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Returns the updated product</response>
    /// <response code="404">If product is not found</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        Guid id, 
        [FromBody] UpdateProductDto updateProductDto, 
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid product ID");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Updating product with ID: {ProductId}", id);
        var product = await _productService.UpdateAsync(id, updateProductDto, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for update", id);
            return NotFound($"Product with ID {id} not found");
        }

        _logger.LogInformation("Updated product with ID: {ProductId}", id);
        return Ok(product);
    }

    /// <summary>
    /// Partially update a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateProductDto">Product update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Returns the updated product</response>
    /// <response code="404">If product is not found</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> PatchProduct(
        Guid id, 
        [FromBody] UpdateProductDto updateProductDto, 
        CancellationToken cancellationToken = default)
    {
        // PATCH and PUT behave the same in this implementation
        // In a more advanced scenario, you might use JsonPatch for PATCH operations
        return await UpdateProduct(id, updateProductDto, cancellationToken);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Product successfully deleted</response>
    /// <response code="404">If product is not found</response>
    /// <response code="400">If the ID format is invalid</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid product ID");

        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        
        if (!deleted)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            return NotFound($"Product with ID {id} not found");
        }

        _logger.LogInformation("Deleted product with ID: {ProductId}", id);
        return NoContent();
    }

    /// <summary>
    /// Get total product count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of products</returns>
    /// <response code="200">Returns the total count</response>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetProductCount(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product count");
        var count = await _productService.CountAsync(cancellationToken);
        return Ok(count);
    }

    /// <summary>
    /// Check if a product exists
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Existence status</returns>
    /// <response code="200">Returns whether the product exists</response>
    /// <response code="400">If the ID format is invalid</response>
    [HttpHead("{id:guid}")]
    [HttpGet("{id:guid}/exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ProductExists(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid product ID");

        var exists = await _productService.ExistsAsync(id, cancellationToken);
        return Ok(exists);
    }
}
