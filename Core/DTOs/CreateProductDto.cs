using System;
using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace Core.DTOs;

public class CreateProductDto
{
    [Required] public string Name { get; set; } = string.Empty;

    [Required] public string Description { get; set; } = string.Empty;
    
    
     public decimal Price { get; set; }

    // [Required] public string Type { get; set; } = string.Empty;
    // [Required] public string Brand { get; set; } = string.Empty;
    [Required(ErrorMessage = "Please select a brand.")]
    public int ProductBrandId { get; set; } // CHANGED from string Brand

    [Required(ErrorMessage = "Please select a type.")]
    public int ProductTypeId { get; set; } // CHANGED from string Type

    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
    
    
    public int QuantityInStock { get; set; }
    [Required]
    public ProductKind ProductKind { get; set; } = ProductKind.Simple;

    // This will hold the JSON string of variants from the form
    public string? VariantsJson { get; set; }
}
