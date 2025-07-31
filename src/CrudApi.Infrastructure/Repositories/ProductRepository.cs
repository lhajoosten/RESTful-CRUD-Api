using Microsoft.EntityFrameworkCore;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using CrudApi.Infrastructure.Data;

namespace CrudApi.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategory)
            .Where(p => p.ProductCategory != null && p.ProductCategory.Name.ToLower() == category.ToLower())
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && 
                       (p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm) ||
                        (p.ProductCategory != null && p.ProductCategory.Name.Contains(searchTerm))))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllWithCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategory)
            .ToListAsync(cancellationToken);
    }
}
