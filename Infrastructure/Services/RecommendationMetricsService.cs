using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class RecommendationMetricsService : IRecommendationMetricsService
{
    private readonly StoreContext _context;
    private readonly ILogger<RecommendationMetricsService> _logger;

    public RecommendationMetricsService(StoreContext context, ILogger<RecommendationMetricsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RecordImpressionAsync(string userId, int recommendedProductId,
        RecommendationStrategy strategy, int position, int? sourceProductId = null, int? experimentId = null)
    {
        await RecordEventAsync(userId, recommendedProductId, RecommendationEventType.Impression,
            strategy, position, sourceProductId, experimentId);
    }

    public async Task RecordClickAsync(string userId, int recommendedProductId,
        RecommendationStrategy strategy, int position, int? sourceProductId = null, int? experimentId = null)
    {
        await RecordEventAsync(userId, recommendedProductId, RecommendationEventType.Click,
            strategy, position, sourceProductId, experimentId);
    }

    public async Task RecordAddToCartAsync(string userId, int recommendedProductId,
        RecommendationStrategy strategy, int? experimentId = null)
    {
        await RecordEventAsync(userId, recommendedProductId, RecommendationEventType.AddToCart,
            strategy, 0, null, experimentId);
    }

    public async Task RecordPurchaseAsync(string userId, int recommendedProductId,
        RecommendationStrategy strategy, int? experimentId = null)
    {
        await RecordEventAsync(userId, recommendedProductId, RecommendationEventType.Purchase,
            strategy, 0, null, experimentId);
    }

    public async Task<ExperimentMetrics> GetExperimentMetricsAsync(int experimentId)
    {
        var experiment = await _context.ABTestExperiments
            .Include(e => e.Assignments)
            .FirstOrDefaultAsync(e => e.Id == experimentId);

        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        var controlUserIds = experiment.Assignments
            .Where(a => !a.IsTreatment)
            .Select(a => a.UserId)
            .ToList();

        var treatmentUserIds = experiment.Assignments
            .Where(a => a.IsTreatment)
            .Select(a => a.UserId)
            .ToList();

        // Get recommendation events for this experiment
        var events = await _context.RecommendationEvents
            .Where(e => e.ExperimentId == experimentId)
            .ToListAsync();

        var controlEvents = events.Where(e => controlUserIds.Contains(e.UserId)).ToList();
        var treatmentEvents = events.Where(e => treatmentUserIds.Contains(e.UserId)).ToList();

        // Get order data for conversion and AOV
        var experimentStart = experiment.StartDate;
        var experimentEnd = experiment.EndDate ?? DateTime.UtcNow;

        var controlOrders = await _context.Set<Order>()
            .Where(o => controlUserIds.Contains(o.BuyerEmail) &&
                       o.OrderDate >= experimentStart && o.OrderDate <= experimentEnd)
            .ToListAsync();

        var treatmentOrders = await _context.Set<Order>()
            .Where(o => treatmentUserIds.Contains(o.BuyerEmail) &&
                       o.OrderDate >= experimentStart && o.OrderDate <= experimentEnd)
            .ToListAsync();

        var metrics = new ExperimentMetrics
        {
            ExperimentId = experimentId,
            ExperimentName = experiment.Name,

            ControlUsers = controlUserIds.Count,
            ControlImpressions = controlEvents.Count(e => e.EventType == RecommendationEventType.Impression),
            ControlClicks = controlEvents.Count(e => e.EventType == RecommendationEventType.Click),
            ControlAddToCarts = controlEvents.Count(e => e.EventType == RecommendationEventType.AddToCart),
            ControlPurchases = controlEvents.Count(e => e.EventType == RecommendationEventType.Purchase),
            ControlOrders = controlOrders.Count,
            ControlRevenue = controlOrders.Sum(o => o.Subtotal - o.Discount),

            TreatmentUsers = treatmentUserIds.Count,
            TreatmentImpressions = treatmentEvents.Count(e => e.EventType == RecommendationEventType.Impression),
            TreatmentClicks = treatmentEvents.Count(e => e.EventType == RecommendationEventType.Click),
            TreatmentAddToCarts = treatmentEvents.Count(e => e.EventType == RecommendationEventType.AddToCart),
            TreatmentPurchases = treatmentEvents.Count(e => e.EventType == RecommendationEventType.Purchase),
            TreatmentOrders = treatmentOrders.Count,
            TreatmentRevenue = treatmentOrders.Sum(o => o.Subtotal - o.Discount),
        };

        // Calculate p-values using z-test for proportions
        metrics.PValueConversion = CalculateProportionZTestPValue(
            metrics.ControlOrders, metrics.ControlUsers,
            metrics.TreatmentOrders, metrics.TreatmentUsers);

        metrics.PValueAOV = CalculateTTestPValue(
            controlOrders.Select(o => (double)(o.Subtotal - o.Discount)).ToList(),
            treatmentOrders.Select(o => (double)(o.Subtotal - o.Discount)).ToList());

        return metrics;
    }

    public async Task<RecommendationSystemMetrics> GetSystemMetricsAsync(DateTime from, DateTime to)
    {
        var events = await _context.RecommendationEvents
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .ToListAsync();

        var metrics = new RecommendationSystemMetrics
        {
            From = from,
            To = to,
            TotalImpressions = events.Count(e => e.EventType == RecommendationEventType.Impression),
            TotalClicks = events.Count(e => e.EventType == RecommendationEventType.Click),
            TotalAddToCarts = events.Count(e => e.EventType == RecommendationEventType.AddToCart),
            TotalPurchases = events.Count(e => e.EventType == RecommendationEventType.Purchase),
        };

        // Break down by strategy
        metrics.ByStrategy = events
            .GroupBy(e => e.Strategy)
            .Select(g => new StrategyMetrics
            {
                Strategy = g.Key,
                Impressions = g.Count(e => e.EventType == RecommendationEventType.Impression),
                Clicks = g.Count(e => e.EventType == RecommendationEventType.Click),
                Purchases = g.Count(e => e.EventType == RecommendationEventType.Purchase),
            })
            .ToList();

        return metrics;
    }

    #region Private Methods

    private async Task RecordEventAsync(string userId, int recommendedProductId,
        RecommendationEventType eventType, RecommendationStrategy strategy,
        int position, int? sourceProductId, int? experimentId)
    {
        var evt = new RecommendationEvent
        {
            UserId = userId,
            RecommendedProductId = recommendedProductId,
            SourceProductId = sourceProductId,
            EventType = eventType,
            Strategy = strategy,
            Position = position,
            ExperimentId = experimentId,
            Timestamp = DateTime.UtcNow
        };

        _context.RecommendationEvents.Add(evt);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Two-proportion z-test for conversion rate comparison.
    /// </summary>
    private static double CalculateProportionZTestPValue(
        int successesA, int totalA, int successesB, int totalB)
    {
        if (totalA == 0 || totalB == 0) return 1.0;

        double pA = (double)successesA / totalA;
        double pB = (double)successesB / totalB;
        double pPooled = (double)(successesA + successesB) / (totalA + totalB);

        if (pPooled == 0 || pPooled == 1) return 1.0;

        double se = Math.Sqrt(pPooled * (1 - pPooled) * (1.0 / totalA + 1.0 / totalB));
        if (se == 0) return 1.0;

        double z = (pB - pA) / se;

        // One-tailed p-value (testing if treatment > control)
        return 1 - NormalCDF(z);
    }

    /// <summary>
    /// Welch's t-test for AOV comparison.
    /// </summary>
    private static double CalculateTTestPValue(List<double> groupA, List<double> groupB)
    {
        if (groupA.Count < 2 || groupB.Count < 2) return 1.0;

        double meanA = groupA.Average();
        double meanB = groupB.Average();
        double varA = groupA.Sum(x => (x - meanA) * (x - meanA)) / (groupA.Count - 1);
        double varB = groupB.Sum(x => (x - meanB) * (x - meanB)) / (groupB.Count - 1);

        double se = Math.Sqrt(varA / groupA.Count + varB / groupB.Count);
        if (se == 0) return 1.0;

        double t = (meanB - meanA) / se;

        // Approximate degrees of freedom (Welch-Satterthwaite)
        double numerator = Math.Pow(varA / groupA.Count + varB / groupB.Count, 2);
        double denominator = Math.Pow(varA / groupA.Count, 2) / (groupA.Count - 1) +
                           Math.Pow(varB / groupB.Count, 2) / (groupB.Count - 1);
        double df = numerator / denominator;

        // Approximate p-value using normal distribution (good for df > 30)
        return 1 - NormalCDF(t);
    }

    /// <summary>
    /// Approximation of the standard normal CDF.
    /// </summary>
    private static double NormalCDF(double x)
    {
        // Abramowitz and Stegun approximation
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x) / Math.Sqrt(2);

        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return 0.5 * (1.0 + sign * y);
    }

    #endregion
}
