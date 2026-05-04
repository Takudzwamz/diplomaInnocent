using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class EmailTemplate : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "New Coupon Announcement"

    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty; // e.g., "A {DiscountValue} Offer Just For You!"

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Body { get; set; } = string.Empty; // The full HTML with placeholders
}