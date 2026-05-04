namespace Core.DTOs;

public class ProductImageDto
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool IsMain { get; set; }
}