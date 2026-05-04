namespace Core.DTOs;

public class ProductReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public required string ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; }
}