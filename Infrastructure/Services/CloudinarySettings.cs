namespace Infrastructure.Services;

/// <summary>
/// Maps to the CloudinarySettings section in appsettings.json
/// </summary>
public class CloudinarySettings
{
    public string? CloudName { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
}
