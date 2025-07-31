using Microsoft.EntityFrameworkCore;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using CrudApi.Infrastructure.Data;

namespace CrudApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Category operations
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get categories with their child categories
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync()
    {
        return await _context.Categories
            .Include(c => c.ChildCategories)
            .ThenInclude(cc => cc.ChildCategories)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
    }

    /// <summary>
    /// Get root categories (no parent)
    /// </summary>
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.ChildCategories)
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get categories by parent ID
    /// </summary>
    public async Task<IEnumerable<Category>> GetByParentIdAsync(Guid parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Check if slug already exists
    /// </summary>
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Slug == slug && !c.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    // Additional methods for test compatibility
    public async Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.Where(c => c.Slug == slug && !c.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
