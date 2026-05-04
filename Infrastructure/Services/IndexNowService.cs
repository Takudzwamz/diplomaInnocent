using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Infrastructure.Services;

public class IndexNowService : IIndexNowService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISiteSettingsService _siteSettings;
    private readonly ILogger<IndexNowService> _logger;
    
    private const string IndexNowEndpoint = "https://api.indexnow.org/IndexNow";

    public IndexNowService(IHttpClientFactory httpClientFactory, ISiteSettingsService siteSettings, ILogger<IndexNowService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _siteSettings = siteSettings;
        _logger = logger;
    }

    public async Task SubmitUrlsAsync(List<string> urls)
    {
        var settings = await _siteSettings.GetSettingsAsync();
        var host = new Uri(settings.GetValueOrDefault("PublicUrl") ?? "https://localhost").Host;
        var apiKey = settings.GetValueOrDefault("IndexNow_ApiKey");
        var keyLocation = $"{settings.GetValueOrDefault("PublicUrl")?.TrimEnd('/')}/{apiKey}.txt";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("IndexNow submission skipped: IndexNow_ApiKey is not configured.");
            return;
        }

        var payload = new
        {
            host = host,
            key = apiKey,
            keyLocation = keyLocation,
            urlList = urls.Distinct().ToList()
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(IndexNowEndpoint, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("IndexNow: Successfully submitted {UrlCount} URLs.", payload.urlList.Count);
            }
            else
            {
                _logger.LogWarning("IndexNow: API submission failed with status {StatusCode}. Response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IndexNow: An exception occurred while submitting URLs.");
        }
    }
}