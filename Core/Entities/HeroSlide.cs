using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class HeroSlide : BaseEntity
{
    
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Subtext { get; set; } = string.Empty;

    [Required, Display(Name = "Button Link")]
    public string ButtonLink { get; set; } = "/Products";

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}