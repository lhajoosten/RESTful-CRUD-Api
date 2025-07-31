namespace CrudApi.Application.DTOs;

/// <summary>
/// Product tag data transfer object
/// </summary>
public class ProductTagDto
{
    /// <summary>
    /// Tag ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tag name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tag color
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Number of products with this tag
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
