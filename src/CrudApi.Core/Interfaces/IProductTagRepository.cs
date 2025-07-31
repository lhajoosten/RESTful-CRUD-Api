using CrudApi.Core.Common;
using CrudApi.Core.Entities;

namespace CrudApi.Core.Interfaces;

/// <summary>
/// Repository interface for ProductTag operations
/// </summary>
public interface IProductTagRepository : IRepository<ProductTag>
{
    /// <summary>
    /// Get tags by product ID
    /// </summary>
    Task<IEnumerable<ProductTag>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Get popular tags (most used)
    /// </summary>
    Task<IEnumerable<ProductTag>> GetPopularTagsAsync(int count = 10);
}
