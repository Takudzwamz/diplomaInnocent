using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class AddTrackingEventDto
{
    [Required]
    public required string NewStatus { get; set; }
    public string? Notes { get; set; }
}