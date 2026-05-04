using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Recommendations;

public class EvaluationModel : PageModel
{
    private readonly IOfflineMetricsService _offlineMetrics;
    private readonly IRecommendationMetricsService _onlineMetrics;

    public EvaluationModel(
        IOfflineMetricsService offlineMetrics,
        IRecommendationMetricsService onlineMetrics)
    {
        _offlineMetrics = offlineMetrics;
        _onlineMetrics = onlineMetrics;
    }

    public OfflineEvaluationResult? EvaluationResult { get; set; }
    public RecommendationSystemMetrics? OnlineMetrics { get; set; }

    [BindProperty(SupportsGet = true)]
    public int K { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public int Days { get; set; } = 30;

    [BindProperty(SupportsGet = true)]
    public double Split { get; set; } = 0.8;

    public bool HasRun { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Офлайн-оценка";

        // Always show online metrics for comparison
        try
        {
            OnlineMetrics = await _onlineMetrics.GetSystemMetricsAsync(
                DateTime.UtcNow.AddDays(-Days), DateTime.UtcNow);
        }
        catch { }
    }

    public async Task<IActionResult> OnPostRunEvaluationAsync(int k, int days, double split)
    {
        ViewData["Title"] = "Offline Evaluation";
        K = k;
        Days = days;
        Split = split;

        var request = new OfflineEvaluationRequest
        {
            From = DateTime.UtcNow.AddDays(-days),
            To = DateTime.UtcNow,
            TrainTestSplit = split,
            K = k
        };

        EvaluationResult = await _offlineMetrics.EvaluateAsync(request);
        HasRun = true;

        try
        {
            OnlineMetrics = await _onlineMetrics.GetSystemMetricsAsync(
                DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
        }
        catch { }

        return Page();
    }
}
