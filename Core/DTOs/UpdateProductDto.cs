using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace Core.DTOs;

public class UpdateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    // public string Type { get; set; }
    // public string Brand { get; set; }
    [Required]
    public int ProductTypeId { get; set; } // ADDED

    [Required]
    public int ProductBrandId { get; set; } // ADDED

    [Required]
    public int CategoryId { get; set; } // ADDED

    public int QuantityInStock { get; set; }

    public ProductKind ProductKind { get; set; }
    public string? VariantsJson { get; set; }
}