using System;

namespace Core.Entities;

public class CartItem
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public required string PictureUrl { get; set; }
    public required string ProductBrand { get; set; }
    public required string ProductType { get; set; }
    public required string ProductCategory { get; set; }
    public string? SelectedOptions { get; set; }
}