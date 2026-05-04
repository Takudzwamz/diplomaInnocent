using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.AIInsights;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IAdminAIService _aiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IAdminAIService aiService, ILogger<IndexModel> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public bool IsAIEnabled => _aiService.IsEnabled;

    // Data properties - loaded via AJAX for better UX
    public SalesForecastResult? SalesForecast { get; set; }
    public InventoryInsightsResult? InventoryInsights { get; set; }
    public PricingRecommendationsResult? PricingRecommendations { get; set; }
    public CustomerInsightsResult? CustomerInsights { get; set; }
    public ReviewAnalysisResult? ReviewAnalysis { get; set; }

    public void OnGet()
    {
        ViewData["Title"] = "AI Insights";
    }

    // AJAX endpoints for loading each insight section
    public async Task<IActionResult> OnGetSalesForecastAsync()
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        try
        {
            var result = await _aiService.GenerateSalesForecastAsync(new SalesForecastRequest 
            { 
                ForecastDays = 30,
                IncludeSeasonalAnalysis = true 
            });
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sales forecast");
            return new JsonResult(new { error = "Failed to load sales forecast" });
        }
    }

    public async Task<IActionResult> OnGetInventoryInsightsAsync()
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        try
        {
            var result = await _aiService.GenerateInventoryInsightsAsync();
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory insights");
            return new JsonResult(new { error = "Failed to load inventory insights" });
        }
    }

    public async Task<IActionResult> OnGetPricingRecommendationsAsync()
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        try
        {
            var result = await _aiService.GeneratePricingRecommendationsAsync();
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing recommendations");
            return new JsonResult(new { error = "Failed to load pricing recommendations" });
        }
    }

    public async Task<IActionResult> OnGetCustomerInsightsAsync()
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        try
        {
            var result = await _aiService.GenerateCustomerInsightsAsync();
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer insights");
            return new JsonResult(new { error = "Failed to load customer insights" });
        }
    }

    public async Task<IActionResult> OnGetReviewAnalysisAsync()
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        try
        {
            var result = await _aiService.AnalyzeAllReviewsAsync();
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading review analysis");
            return new JsonResult(new { error = "Failed to load review analysis" });
        }
    }
}
