namespace Core.Entities;

public class ProductReview : BaseEntity
{
    public int Rating { get; set; } // e.g., 1-5 stars
    public string? Comment { get; set; }
    public required string ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    // Foreign key relationship to Product
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Foreign key relationship to AppUser
    public required string AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;
}