using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class ProductOptionDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<ProductOptionValueDto> Values { get; set; } = [];
}

public class ProductOptionValueDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ColorHex { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public int? ImageId { get; set; }

    // A list of the IDs of the ProductOptionValues that make up this variant.
    // e.g., [1 (for Small), 5 (for Red)]
    public List<int> OptionValueIds { get; set; } = [];
    public decimal? DiscountedPrice { get; set; }
}

public class ProductOptionForListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int ValueCount { get; set; }
}

public class CreateOrUpdateOptionDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    // A comma-separated string of values, e.g., "Small, Medium, Large"
    // For simple text values (no colors)
    public string? Values { get; set; }
    
    // Structured list for values with color support
    // Each item contains Name and optional ColorHex
    public List<OptionValueInputDto>? OptionValues { get; set; }
}

public class OptionValueInputDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
}