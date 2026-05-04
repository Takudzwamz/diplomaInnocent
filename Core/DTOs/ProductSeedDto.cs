using Core.Entities;

namespace Core.DTOs;

public class ProductSeedDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Type { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    public int QuantityInStock { get; set; }
    public List<ProductImage> Images { get; set; }
}

public class ProductOptionSeedDto
{
    public string Name { get; set; }
    public List<ProductOptionValueSeedDto> Values { get; set; }
}

public class ProductOptionValueSeedDto
{
    public string Name { get; set; }
}

public class ProductVariableSeedDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Type { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    public List<string> Options { get; set; }
    public List<VariantSeedDto> Variants { get; set; }
}

public class VariantSeedDto
{
    public List<string> OptionValues { get; set; }
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public string ImageUrl { get; set; }
}