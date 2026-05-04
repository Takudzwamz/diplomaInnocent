using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// Service for tracking and computing recommendation system metrics.
/// Supports the thesis experimental evaluation (Chapter 4).
/// </summary>
public interface IRecommendationMetricsService
{
    /// <summary>
    /// Records a recommendation impression (shown to user).
    /// </summary>
    Task RecordImpressionAsync(string userId, int recommendedProductId, 
        RecommendationStrategy strategy, int position, int? sourceProductId = null, int? experimentId = null);

    /// <summary>
    /// Records a click on a recommended product.
    /// </summary>
    Task RecordClickAsync(string userId, int recommendedProductId, 
        RecommendationStrategy strategy, int position, int? sourceProductId = null, int? experimentId = null);

    /// <summary>
    /// Records an add-to-cart from a recommendation.
    /// </summary>
    Task RecordAddToCartAsync(string userId, int recommendedProductId, 
        RecommendationStrategy strategy, int? experimentId = null);

    /// <summary>
    /// Records a purchase attributed to a recommendation.
    /// </summary>
    Task RecordPurchaseAsync(string userId, int recommendedProductId, 
        RecommendationStrategy strategy, int? experimentId = null);

    /// <summary>
    /// Gets aggregated metrics for a specific experiment.
    /// </summary>
    Task<ExperimentMetrics> GetExperimentMetricsAsync(int experimentId);

    /// <summary>
    /// Gets overall recommendation system metrics for a date range.
    /// </summary>
    Task<RecommendationSystemMetrics> GetSystemMetricsAsync(DateTime from, DateTime to);
}

/// <summary>
/// Aggregated metrics for an A/B test experiment (control vs treatment).
/// </summary>
public class ExperimentMetrics
{
    public int ExperimentId { get; set; }
    public string ExperimentName { get; set; } = string.Empty;

    // Control group metrics
    public int ControlUsers { get; set; }
    public int ControlImpressions { get; set; }
    public int ControlClicks { get; set; }
    public int ControlAddToCarts { get; set; }
    public int ControlPurchases { get; set; }
    public int ControlOrders { get; set; }
    public decimal ControlRevenue { get; set; }
    public double ControlCTR => ControlImpressions > 0 ? (double)ControlClicks / ControlImpressions : 0;
    public double ControlConversionRate => ControlUsers > 0 ? (double)ControlOrders / ControlUsers : 0;
    public decimal ControlAOV => ControlOrders > 0 ? ControlRevenue / ControlOrders : 0;

    // Treatment group metrics
    public int TreatmentUsers { get; set; }
    public int TreatmentImpressions { get; set; }
    public int TreatmentClicks { get; set; }
    public int TreatmentAddToCarts { get; set; }
    public int TreatmentPurchases { get; set; }
    public int TreatmentOrders { get; set; }
    public decimal TreatmentRevenue { get; set; }
    public double TreatmentCTR => TreatmentImpressions > 0 ? (double)TreatmentClicks / TreatmentImpressions : 0;
    public double TreatmentConversionRate => TreatmentUsers > 0 ? (double)TreatmentOrders / TreatmentUsers : 0;
    public decimal TreatmentAOV => TreatmentOrders > 0 ? TreatmentRevenue / TreatmentOrders : 0;

    // Relative improvements
    public double CTRLift => ControlCTR > 0 ? (TreatmentCTR - ControlCTR) / ControlCTR * 100 : 0;
    public double ConversionLift => ControlConversionRate > 0 
        ? (TreatmentConversionRate - ControlConversionRate) / ControlConversionRate * 100 : 0;
    public double AOVLift => ControlAOV > 0 
        ? (double)((TreatmentAOV - ControlAOV) / ControlAOV * 100) : 0;

    // Statistical significance
    public double PValueConversion { get; set; }
    public double PValueAOV { get; set; }
    public bool IsStatisticallySignificant => PValueConversion < 0.05;
}

/// <summary>
/// Overall recommendation system metrics.
/// </summary>
public class RecommendationSystemMetrics
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public int TotalAddToCarts { get; set; }
    public int TotalPurchases { get; set; }
    public double OverallCTR => TotalImpressions > 0 ? (double)TotalClicks / TotalImpressions : 0;
    public double AddToCartRate => TotalClicks > 0 ? (double)TotalAddToCarts / TotalClicks : 0;
    public double PurchaseRate => TotalAddToCarts > 0 ? (double)TotalPurchases / TotalAddToCarts : 0;

    /// <summary>
    /// Metrics broken down by strategy.
    /// </summary>
    public List<StrategyMetrics> ByStrategy { get; set; } = [];
}

public class StrategyMetrics
{
    public RecommendationStrategy Strategy { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Purchases { get; set; }
    public double CTR => Impressions > 0 ? (double)Clicks / Impressions : 0;
}
