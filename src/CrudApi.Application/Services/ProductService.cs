using AutoMapper;
using CrudApi.Application.DTOs;
using CrudApi.Application.Interfaces;
using CrudApi.Core.Common;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;

namespace CrudApi.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByCategoryAsync(category, cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockProductsAsync(threshold, cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(createProductDto);
        var createdProduct = await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (existingProduct == null)
            return null;

        _mapper.Map(updateProductDto, existingProduct);
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.UpdateAsync(existingProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _productRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return false;

        await _productRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _productRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepository.CountAsync(cancellationToken);
    }
}
