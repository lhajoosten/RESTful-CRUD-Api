using CrudApi.Application.DTOs;

namespace CrudApi.Application.Interfaces;

/// <summary>
/// Service interface for Category operations
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Get all categories
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);

    /// <summary>
    /// Get category by slug
    /// </summary>
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug);

    /// <summary>
    /// Get categories with their children
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetCategoriesWithChildrenAsync();

    /// <summary>
    /// Get root categories (no parent)
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();

    /// <summary>
    /// Get categories by parent ID
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(Guid parentId);

    /// <summary>
    /// Get products in a category
    /// </summary>
    Task<IEnumerable<ProductDto>> GetCategoryProductsAsync(Guid categoryId);

    /// <summary>
    /// Create a new category
    /// </summary>
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

    /// <summary>
    /// Update an existing category
    /// </summary>
    Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto);

    /// <summary>
    /// Delete a category
    /// </summary>
    Task<bool> DeleteCategoryAsync(Guid id);
}
