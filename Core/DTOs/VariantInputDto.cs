namespace Core.DTOs;

// DTO for deserializing the variant data from the form
public class VariantInputDto
{
    // ADD THIS ID PROPERTY
    public int Id { get; set; }

    public int? ImageId { get; set; }

    public List<int> ValueIds { get; set; } = [];
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public string? Sku { get; set; }
}