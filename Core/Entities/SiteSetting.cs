using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class SiteSetting : BaseEntity
{
    [Required]
    public string Key { get; set; } = string.Empty; // e.g., "StoreName", "StoreLogoUrl"
    public string? Value { get; set; }
}