using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Coupon : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty; // e.g., "SUMMER25"

    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    // Discount can be a fixed amount OR a percentage.
    [Range(0.01, 10000)]
    public decimal? AmountOff { get; set; }

    [Range(0.01, 100)]
    public decimal? PercentOff { get; set; }

    // Activation & Time Limits
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Usage Limits
    public int? UsageLimit { get; set; } // Max number of times this coupon can be used in total
    public int UsageCount { get; set; } = 0; // How many times it has been used
    public bool FirstTimeCustomerOnly { get; set; } = false;

    public bool LimitOnePerCustomer { get; set; } = false;

    public List<CouponProduct> ApplicableProducts { get; set; } = [];
}