using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Content;

public class BannerInputModel
{
    [Required]
    public string CouponCode { get; set; } = string.Empty;

    [Required]
    public string DiscountValue { get; set; } = string.Empty; // e.g., "R100 off" or "25%"

    [Required]
    public string MainText { get; set; } = string.Empty; // e.g., "your first purchase!"
}