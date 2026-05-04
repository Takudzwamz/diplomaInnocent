using System;

namespace Core.Entities.OrderAggregate;

public class ProductItemOrdered
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public required string ProductName { get; set; }
    public required string PictureUrl { get; set; }
    public string? SelectedOptions { get; set; }
}
