using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Recommendations;

public class IndexModel : PageModel
{
    private readonly IABTestService _abTestService;
    private readonly IRecommendationMetricsService _metricsService;
    private readonly StoreContext _context;

    public IndexModel(
        IABTestService abTestService,
        IRecommendationMetricsService metricsService,
        StoreContext context)
    {
        _abTestService = abTestService;
        _metricsService = metricsService;
        _context = context;
    }

    public ABTestExperiment? ActiveExperiment { get; set; }
    public ExperimentMetrics? Metrics { get; set; }
    public RecommendationSystemMetrics? SystemMetrics { get; set; }
    public int TotalInteractions { get; set; }
    public int TotalUsers { get; set; }
    public int TotalExperiments { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Рекомендации и A/B-тесты";

        ActiveExperiment = await _abTestService.GetActiveExperimentAsync();

        if (ActiveExperiment != null)
        {
            try
            {
                Metrics = await _metricsService.GetExperimentMetricsAsync(ActiveExperiment.Id);
            }
            catch { }
        }

        try
        {
            SystemMetrics = await _metricsService.GetSystemMetricsAsync(
                DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        }
        catch { }

        TotalInteractions = await _context.UserInteractions.CountAsync();
        TotalUsers = await _context.UserInteractions.Select(i => i.UserId).Distinct().CountAsync();
        TotalExperiments = await _context.ABTestExperiments.CountAsync();
    }

    public async Task<IActionResult> OnPostCreateExperimentAsync(string name, string description,
        string controlStrategy, string treatmentStrategy, int treatmentPercentage)
    {
        if (!Enum.TryParse<RecommendationStrategy>(controlStrategy, out var control))
            control = RecommendationStrategy.Popular;

        if (!Enum.TryParse<RecommendationStrategy>(treatmentStrategy, out var treatment))
            treatment = RecommendationStrategy.Adaptive;

        await _abTestService.CreateExperimentAsync(
            name, description, control, treatment, treatmentPercentage,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(14));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEndExperimentAsync(int experimentId)
    {
        await _abTestService.EndExperimentAsync(experimentId);
        return RedirectToPage();
    }
}
