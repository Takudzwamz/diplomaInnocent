using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class CreateReviewDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }
}