using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a specific, purchasable combination of options for a product.
/// e.g., A T-Shirt in Size "Small" and Color "Red".
/// </summary>
public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int QuantityInStock { get; set; }
    public string? Sku { get; set; }

    // The combination of option values defining this variant
    public List<ProductOptionValue> OptionValues { get; set; } = [];

    // A variant can have its own specific image
    public int? ImageId { get; set; }
    public ProductImage? Image { get; set; }
}