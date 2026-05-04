using System;
using System.Text.Json.Serialization;

namespace Core.Entities;

public class AppCoupon
{
    
    public required string Name { get; set; }
    public decimal? AmountOff { get; set; }
    public decimal? PercentOff { get; set; }
    public required string PromotionCode { get; set; }
    public required string CouponId { get; set; }
    public List<int> ApplicableProductIds { get; set; } = [];
}
