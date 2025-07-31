using CrudApi.Core.Common;

namespace CrudApi.Core.Entities;

/// <summary>
/// Represents a product category
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category slug for URLs
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Parent category ID for hierarchical categories
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Parent category navigation property
    /// </summary>
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// Child categories
    /// </summary>
    public List<Category> ChildCategories { get; set; } = new();

    /// <summary>
    /// Products in this category
    /// </summary>
    public List<Product> Products { get; set; } = new();

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category metadata (SEO, etc.)
    /// </summary>
    public string? MetaData { get; set; }
}
