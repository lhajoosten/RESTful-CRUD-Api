using Microsoft.EntityFrameworkCore;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using CrudApi.Infrastructure.Data;

namespace CrudApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductTag operations
/// </summary>
public class ProductTagRepository : Repository<ProductTag>, IProductTagRepository
{
    public ProductTagRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get tags by product ID
    /// </summary>
    public async Task<IEnumerable<ProductTag>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductTags
            .Where(t => t.Products.Any(p => p.Id == productId) && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get popular tags (most used)
    /// </summary>
    public async Task<IEnumerable<ProductTag>> GetPopularTagsAsync(int count = 10)
    {
        return await _context.ProductTags
            .Include(t => t.Products)
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.Products.Count(p => !p.IsDeleted))
            .Take(count)
            .ToListAsync();
    }
}
