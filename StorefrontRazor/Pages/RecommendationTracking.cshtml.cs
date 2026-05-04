using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace StorefrontRazor.Pages;

/// <summary>
/// Handles recommendation system AJAX calls: interaction tracking and click recording.
/// All handlers return JSON and are invoked from JavaScript on the frontend.
/// </summary>
public class RecommendationTrackingModel : PageModel
{
    private readonly IUserInteractionService _interactionService;
    private readonly IRecommendationMetricsService _metricsService;
    private readonly IABTestService _abTestService;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public RecommendationTrackingModel(
        IUserInteractionService interactionService,
        IRecommendationMetricsService metricsService,
        IABTestService abTestService,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _interactionService = interactionService;
        _metricsService = metricsService;
        _abTestService = abTestService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public void OnGet() { }

    /// <summary>
    /// Tracks a user interaction (view, click, add-to-cart, etc.).
    /// Called via AJAX: POST /RecommendationTracking?handler=Track
    /// </summary>
    public async Task<IActionResult> OnPostTrackAsync(int productId, string type, string? sessionId, int? durationSeconds)
    {
        if (!_signInManager.IsSignedIn(User))
            return new JsonResult(new { ok = false });

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);
        if (user == null) return new JsonResult(new { ok = false });

        if (!Enum.TryParse<InteractionType>(type, true, out var interactionType))
            return new JsonResult(new { ok = false, error = "Неверный тип" });

        await _interactionService.TrackInteractionAsync(user.Id, productId, interactionType, sessionId, durationSeconds);
        return new JsonResult(new { ok = true });
    }

    /// <summary>
    /// Records a click on a recommended product (for CTR calculation).
    /// Called via AJAX: POST /RecommendationTracking?handler=Click
    /// </summary>
    public async Task<IActionResult> OnPostClickAsync(int productId, string strategy, int position, int? sourceProductId)
    {
        if (!_signInManager.IsSignedIn(User))
            return new JsonResult(new { ok = false });

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);
        if (user == null) return new JsonResult(new { ok = false });

        if (!Enum.TryParse<RecommendationStrategy>(strategy, true, out var strat))
            strat = RecommendationStrategy.Adaptive;

        var experiment = await _abTestService.GetActiveExperimentAsync();

        await _metricsService.RecordClickAsync(user.Id, productId, strat, position, sourceProductId, experiment?.Id);
        await _interactionService.TrackInteractionAsync(user.Id, productId, InteractionType.RecommendationClick);

        return new JsonResult(new { ok = true });
    }

    /// <summary>
    /// Records an impression when recommendations are displayed.
    /// Called via AJAX: POST /RecommendationTracking?handler=Impression
    /// </summary>
    public async Task<IActionResult> OnPostImpressionAsync(int productId, string strategy, int position, int? sourceProductId)
    {
        if (!_signInManager.IsSignedIn(User))
            return new JsonResult(new { ok = false });

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);
        if (user == null) return new JsonResult(new { ok = false });

        if (!Enum.TryParse<RecommendationStrategy>(strategy, true, out var strat))
            strat = RecommendationStrategy.Adaptive;

        var experiment = await _abTestService.GetActiveExperimentAsync();

        await _metricsService.RecordImpressionAsync(user.Id, productId, strat, position, sourceProductId, experiment?.Id);
        return new JsonResult(new { ok = true });
    }
}
