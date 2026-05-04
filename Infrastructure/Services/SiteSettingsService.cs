using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private const string SettingsCacheKey = "SiteSettings";

    public SiteSettingsService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Dictionary<string, string?>> GetSettingsAsync()
    {
        // Try to get the settings from the cache first
        if (_cache.TryGetValue(SettingsCacheKey, out Dictionary<string, string?>? settings))
        {
            return settings ?? new Dictionary<string, string?>();
        }

        // If not in cache, fetch from the database
        var settingsFromDb = await _unitOfWork.Repository<SiteSetting>().ListAllAsync();
        settings = settingsFromDb.ToDictionary(s => s.Key, s => s.Value);

        // Store in cache for 1 hour
        _cache.Set(SettingsCacheKey, settings, TimeSpan.FromHours(1));

        return settings;
    }

    public void ClearCache()
    {
        _cache.Remove(SettingsCacheKey);
    }
}