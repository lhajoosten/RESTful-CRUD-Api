using CrudApi.Core.Common;
using CrudApi.Core.Entities;

namespace CrudApi.Core.Interfaces;

/// <summary>
/// Repository interface for Category operations
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Get categories with their child categories
    /// </summary>
    Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync();

    /// <summary>
    /// Get category by slug
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug);

    /// <summary>
    /// Get root categories (no parent)
    /// </summary>
    Task<IEnumerable<Category>> GetRootCategoriesAsync();

    /// <summary>
    /// Get categories by parent ID
    /// </summary>
    Task<IEnumerable<Category>> GetByParentIdAsync(Guid parentId);

    /// <summary>
    /// Check if slug already exists
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
}
