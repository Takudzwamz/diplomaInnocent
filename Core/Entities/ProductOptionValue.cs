namespace Core.Entities;

/// <summary>
/// Represents a specific value for an option, e.g., "Small", "Red".
/// </summary>
public class ProductOptionValue : BaseEntity
{
    public required string Name { get; set; }
    
    /// <summary>
    /// Optional hex color code (e.g., "#FF0000" for red). 
    /// When set, the option value will be displayed as a color circle instead of text.
    /// </summary>
    public string? ColorHex { get; set; }
    
    public int ProductOptionId { get; set; }
    public ProductOption ProductOption { get; set; } = null!;
    public List<ProductVariant> Variants { get; set; } = [];
}