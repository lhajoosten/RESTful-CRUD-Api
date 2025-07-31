using AutoMapper;
using CrudApi.Application.DTOs;
using CrudApi.Application.Interfaces;
using CrudApi.Core.Common;
using CrudApi.Core.Entities;
using CrudApi.Core.Interfaces;
using System.Text.RegularExpressions;

namespace CrudApi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesWithChildrenAsync()
    {
        var categories = await _categoryRepository.GetCategoriesWithChildrenAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(Guid parentId)
    {
        var categories = await _categoryRepository.GetByParentIdAsync(parentId);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<IEnumerable<ProductDto>> GetCategoryProductsAsync(Guid categoryId)
    {
        var products = await _productRepository.GetByCategoryIdAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var category = _mapper.Map<Category>(createCategoryDto);
        category.Slug = await GenerateUniqueSlugAsync(createCategoryDto.Name);
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        // Validate parent category if specified
        if (createCategoryDto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(createCategoryDto.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                throw new ArgumentException("Parent category not found.");
            }

            // Check for circular reference
            if (await WouldCreateCircularReferenceAsync(category.Id, createCategoryDto.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("Cannot create circular reference in category hierarchy.");
            }
        }

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return null;
        }

        // Update basic properties
        _mapper.Map(updateCategoryDto, category);
        category.UpdatedAt = DateTime.UtcNow;

        // Update slug if name changed
        if (!string.Equals(category.Name, updateCategoryDto.Name, StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await GenerateUniqueSlugAsync(updateCategoryDto.Name, id);
        }

        // Validate parent category if specified
        if (updateCategoryDto.ParentCategoryId.HasValue)
        {
            if (updateCategoryDto.ParentCategoryId.Value == id)
            {
                throw new InvalidOperationException("Category cannot be its own parent.");
            }

            var parentCategory = await _categoryRepository.GetByIdAsync(updateCategoryDto.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                throw new ArgumentException("Parent category not found.");
            }

            // Check for circular reference
            if (await WouldCreateCircularReferenceAsync(id, updateCategoryDto.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("Cannot create circular reference in category hierarchy.");
            }
        }

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        // Check if category has child categories
        var childCategories = await _categoryRepository.GetByParentIdAsync(id);
        if (childCategories.Any())
        {
            throw new InvalidOperationException("Cannot delete category that has child categories.");
        }

        // Check if category has products
        var products = await _productRepository.GetByCategoryIdAsync(id);
        if (products.Any())
        {
            throw new InvalidOperationException("Cannot delete category that contains products.");
        }

        await _categoryRepository.DeleteAsync(category.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeId = null)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await _categoryRepository.SlugExistsAsync(slug, excludeId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lowercase and replace spaces with hyphens
        var slug = input.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Guid categoryId, Guid parentId)
    {
        var currentParentId = parentId;
        
        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == categoryId)
            {
                return true;
            }

            var parentCategory = await _categoryRepository.GetByIdAsync(currentParentId);
            currentParentId = parentCategory?.ParentCategoryId ?? Guid.Empty;
        }

        return false;
    }
}