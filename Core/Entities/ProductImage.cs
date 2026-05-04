namespace Core.Entities;

public class ProductImage : BaseEntity
{
    public required string Url { get; set; }
    public bool IsMain { get; set; }

    // Foreign key relationship to Product
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}