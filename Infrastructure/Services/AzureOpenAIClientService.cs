using Azure;
using Azure.AI.OpenAI;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ClientModel;

namespace Infrastructure.Services;

/// <summary>
/// Singleton service that manages the Azure OpenAI client connection.
/// Created once at startup and reused across all requests for better performance.
/// Reads configuration from database via ISiteSettingsService using a service scope.
/// </summary>
public class AzureOpenAIClientService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureOpenAIClientService> _logger;
    public AzureOpenAIClient? Client { get; private set; }
    public string? EmbeddingDeployment { get; private set; }
    public string? ChatDeployment { get; private set; }
    public bool IsEnabled { get; private set; }

    public AzureOpenAIClientService(
        IServiceProvider serviceProvider,
        ILogger<AzureOpenAIClientService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeClientAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeClientAsync()
    {
        try
        {
            // Create a scope to resolve the scoped ISiteSettingsService
            using var scope = _serviceProvider.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<ISiteSettingsService>();
            
            var settings = await settingsService.GetSettingsAsync();
            
            if (settings == null)
            {
                _logger.LogWarning("Failed to load site settings. AI recommendations will be disabled.");
                IsEnabled = false;
                return;
            }

            // Check if AI recommendations are enabled
            var enabledValue = settings.GetValueOrDefault("AI_Enabled", "false") ?? "false";
            IsEnabled = enabledValue.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (IsEnabled)
            {
                var endpoint = settings.GetValueOrDefault("AI_Endpoint") ?? "";
                var apiKey = settings.GetValueOrDefault("AI_ApiKey") ?? "";
                EmbeddingDeployment = settings.GetValueOrDefault("AI_EmbeddingDeployment") ?? "text-embedding-ada-002";
                ChatDeployment = settings.GetValueOrDefault("AI_ChatDeployment") ?? "gpt-4o-mini";

                if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
                {
                    try
                    {
                        Client = new AzureOpenAIClient(
                            new Uri(endpoint),
                            new ApiKeyCredential(apiKey));
                        _logger.LogInformation("Azure OpenAI client initialized successfully from database settings");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize Azure OpenAI client");
                        IsEnabled = false;
                    }
                }
                else
                {
                    _logger.LogWarning("Azure OpenAI configuration missing in database. AI recommendations will be disabled.");
                    IsEnabled = false;
                }
            }
            else
            {
                _logger.LogInformation("Azure OpenAI recommendations are disabled in database settings");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading AI settings from database");
            IsEnabled = false;
        }
    }

    /// <summary>
    /// Reinitialize the client with fresh settings from database.
    /// Call this after updating AI settings in the admin panel.
    /// </summary>
    public async Task RefreshSettingsAsync()
    {
        await InitializeClientAsync();
    }
}
