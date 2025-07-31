using CrudApi.Application.DTOs;

namespace CrudApi.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
