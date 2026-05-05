using Core.Entities;

namespace Core.DTOs;

public class ProductSeedDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public List<ProductImage> Images { get; set; } = [];
}

public class ProductOptionSeedDto
{
    public string Name { get; set; } = string.Empty;
    public List<ProductOptionValueSeedDto> Values { get; set; } = [];
}

public class ProductOptionValueSeedDto
{
    public string Name { get; set; } = string.Empty;
}

public class ProductVariableSeedDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public List<VariantSeedDto> Variants { get; set; } = [];
}

public class VariantSeedDto
{
    public List<string> OptionValues { get; set; } = [];
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}