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
            .Where(p => p.Category.ToLower() == category.ToLower())
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
}
