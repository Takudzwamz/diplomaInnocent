using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class ContentBlock : BaseEntity
{
    [Required]
    [MaxLength(100)]
    // A unique, machine-readable key to identify the content (e.g., "home-page-promo", "return-policy")
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty; // A friendly title for the admin panel

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Content { get; set; } = string.Empty; // The actual content (can be text or HTML)

    // A flag to tell our app whether to render the content as raw HTML
    public bool IsHtml { get; set; }
}