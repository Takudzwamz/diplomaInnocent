namespace Core.Interfaces;

public interface ISiteSettingsService
{
    Task<Dictionary<string, string?>> GetSettingsAsync();
    void ClearCache();
}