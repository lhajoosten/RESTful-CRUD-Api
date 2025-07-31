using CrudApi.Core.Common;

namespace CrudApi.Core.Entities;

/// <summary>
/// Represents a product tag for additional categorization
/// </summary>
public class ProductTag : BaseEntity
{
    /// <summary>
    /// Tag name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tag color for UI display
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Products associated with this tag
    /// </summary>
    public List<Product> Products { get; set; } = new();
}
