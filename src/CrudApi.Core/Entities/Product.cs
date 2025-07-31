using CrudApi.Core.Common;

namespace CrudApi.Core.Entities;

/// <summary>
/// Represents a product in the system
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Category ID (foreign key)
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Category navigation property
    /// </summary>
    public Category? ProductCategory { get; set; }

    /// <summary>
    /// Product tags for additional categorization
    /// </summary>
    public List<ProductTag> Tags { get; set; } = new();

    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Product images
    /// </summary>
    public List<ProductImage> Images { get; set; } = new();

    /// <summary>
    /// Primary image URL (legacy - use Images collection)
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Product SKU (Stock Keeping Unit)
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Product weight in grams
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product dimensions (JSON string)
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Minimum stock level for alerts
    /// </summary>
    public int MinStockLevel { get; set; } = 0;

    /// <summary>
    /// Product metadata (specifications, etc.)
    /// </summary>
    public string? MetaData { get; set; }
}
