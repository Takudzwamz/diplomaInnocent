namespace Core.Entities;

/// <summary>
/// Represents an option type, e.g., "Size", "Color".
/// </summary>
public class ProductOption : BaseEntity
{
    public required string Name { get; set; }
    public List<ProductOptionValue> Values { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}