namespace Core.DTOs;

public class TrackingEventDto
{
    public DateTime EventDate { get; set; }
    public required string Status { get; set; }
    public string? Notes { get; set; }
}