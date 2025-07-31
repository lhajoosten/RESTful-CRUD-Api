using Microsoft.AspNetCore.Mvc;
using CrudApi.Application.Interfaces;
using CrudApi.Application.DTOs;
using CrudApi.Api.Models;
using AutoMapper;
using CrudApi.Core.Entities;
using Asp.Versioning;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Categories management controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the CategoriesController
    /// </summary>
    /// <param name="categoryService">Category service</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public CategoriesController(ICategoryService categoryService, IMapper mapper, ILogger<CategoriesController> logger)
        : base()
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all categories with optional hierarchy
    /// </summary>
    /// <param name="includeChildren">Include child categories</param>
    /// <param name="activeOnly">Only active categories</param>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories(
        [FromQuery] bool includeChildren = false,
        [FromQuery] bool activeOnly = true)
    {
        var categories = includeChildren 
            ? await _categoryService.GetCategoriesWithChildrenAsync()
            : await _categoryService.GetAllCategoriesAsync();

        if (activeOnly)
        {
            categories = categories.Where(c => c.IsActive);
        }

        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos, "Categories retrieved successfully"));
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse.ErrorResult("Category not found"));
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category retrieved successfully"));
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    /// <param name="slug">Category slug</param>
    /// <returns>Category details</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryBySlug(string slug)
    {
        var category = await _categoryService.GetCategoryBySlugAsync(slug);
        if (category == null)
        {
            return NotFound(ApiResponse.ErrorResult("Category not found"));
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category retrieved successfully"));
    }

    /// <summary>
    /// Get root categories (no parent)
    /// </summary>
    /// <returns>List of root categories</returns>
    [HttpGet("root")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetRootCategories()
    {
        var categories = await _categoryService.GetRootCategoriesAsync();
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos, "Root categories retrieved successfully"));
    }

    /// <summary>
    /// Get child categories by parent ID
    /// </summary>
    /// <param name="parentId">Parent category ID</param>
    /// <returns>List of child categories</returns>
    [HttpGet("{parentId:guid}/children")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetChildCategories(Guid parentId)
    {
        var categories = await _categoryService.GetChildCategoriesAsync(parentId);
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos, "Child categories retrieved successfully"));
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="categoryDto">Category data</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResult("Invalid category data"));
        }

        var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto);
        var resultDto = _mapper.Map<CategoryDto>(createdCategory);

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = createdCategory.Id },
            ApiResponse<CategoryDto>.SuccessResult(resultDto, "Category created successfully"));
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="categoryDto">Updated category data</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResult("Invalid category data"));
        }

        var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto);
        if (updatedCategory == null)
        {
            return NotFound(ApiResponse.ErrorResult("Category not found"));
        }

        var resultDto = _mapper.Map<CategoryDto>(updatedCategory);
        return Ok(ApiResponse<CategoryDto>.SuccessResult(resultDto, "Category updated successfully"));
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteCategory(Guid id)
    {
        var deleted = await _categoryService.DeleteCategoryAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse.ErrorResult("Category not found"));
        }

        return Ok(ApiResponse.SuccessResult("Category deleted successfully"));
    }

    /// <summary>
    /// Get products in a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("{id:guid}/products")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetCategoryProducts(Guid id)
    {
        var products = await _categoryService.GetCategoryProductsAsync(id);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(productDtos, "Category products retrieved successfully"));
    }
}
