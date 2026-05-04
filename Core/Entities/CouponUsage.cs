namespace Core.Entities;

public class CouponUsage : BaseEntity
{
    public int CouponId { get; set; }
    public required string AppUserId { get; set; }
    public DateTime DateUsed { get; set; } = DateTime.UtcNow;
}