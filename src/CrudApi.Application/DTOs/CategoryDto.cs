using System.ComponentModel.DataAnnotations;

namespace CrudApi.Application.DTOs;

/// <summary>
/// Category data transfer object
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Parent category ID
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Parent category name
    /// </summary>
    public string? ParentCategoryName { get; set; }

    /// <summary>
    /// Child categories
    /// </summary>
    public List<CategoryDto> ChildCategories { get; set; } = new();

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Product count in this category
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Category slug (auto-generated if not provided)
    /// </summary>
    [StringLength(100)]
    public string? Slug { get; set; }

    /// <summary>
    /// Parent category ID
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category metadata
    /// </summary>
    public string? MetaData { get; set; }
}

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Category slug
    /// </summary>
    [StringLength(100)]
    public string? Slug { get; set; }

    /// <summary>
    /// Parent category ID
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Category metadata
    /// </summary>
    public string? MetaData { get; set; }
}
