namespace Core.Entities.OrderAggregate;

public class TrackingEvent : BaseEntity
{
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public required string Status { get; set; }
    public string? Notes { get; set; } // For location or tracking numbers
    
    // Foreign key to the Order
    public int OrderId { get; set; }
}