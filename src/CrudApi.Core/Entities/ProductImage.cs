using CrudApi.Core.Common;

namespace CrudApi.Core.Entities;

/// <summary>
/// Represents a product image
/// </summary>
public class ProductImage : BaseEntity
{
    /// <summary>
    /// Product ID (foreign key)
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product navigation property
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Image URL or file path
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Alternative text for accessibility
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this is the primary image
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Image file size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Image MIME type
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int? Height { get; set; }
}
