namespace Core.Interfaces;

/// <summary>
/// Interface for AI-powered admin features including forecasting, insights, and content generation
/// </summary>
public interface IAdminAIService
{
    /// <summary>
    /// Generates sales forecast and trend analysis
    /// </summary>
    Task<SalesForecastResult?> GenerateSalesForecastAsync(SalesForecastRequest request);

    /// <summary>
    /// Analyzes inventory and provides smart recommendations
    /// </summary>
    Task<InventoryInsightsResult?> GenerateInventoryInsightsAsync();

    /// <summary>
    /// Generates smart pricing recommendations based on sales data and market analysis
    /// </summary>
    Task<PricingRecommendationsResult?> GeneratePricingRecommendationsAsync();

    /// <summary>
    /// Analyzes customer behavior and segments
    /// </summary>
    Task<CustomerInsightsResult?> GenerateCustomerInsightsAsync();

    /// <summary>
    /// Aggregates and analyzes sentiment across all product reviews
    /// </summary>
    Task<ReviewAnalysisResult?> AnalyzeAllReviewsAsync();

    /// <summary>
    /// Generates an AI-powered product description from basic info
    /// </summary>
    Task<ProductDescriptionResult?> GenerateProductDescriptionAsync(ProductDescriptionRequest request);

    /// <summary>
    /// Checks if the AI service is enabled and configured
    /// </summary>
    bool IsEnabled { get; }
}

#region Sales Forecasting

public class SalesForecastRequest
{
    public int ForecastDays { get; set; } = 30;
    public bool IncludeSeasonalAnalysis { get; set; } = true;
}

public class SalesForecastResult
{
    public string Summary { get; set; } = string.Empty;
    public List<ForecastDataPoint> ForecastData { get; set; } = [];
    public decimal PredictedRevenue { get; set; }
    public decimal PredictedGrowthPercent { get; set; }
    public List<string> KeyInsights { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public SeasonalAnalysis? SeasonalTrends { get; set; }
    public string Confidence { get; set; } = "Medium";
}

public class ForecastDataPoint
{
    public DateTime Date { get; set; }
    public decimal PredictedSales { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
}

public class SeasonalAnalysis
{
    public string PeakSeason { get; set; } = string.Empty;
    public string SlowSeason { get; set; } = string.Empty;
    public List<string> SeasonalPatterns { get; set; } = [];
}

#endregion

#region Inventory Insights

public class InventoryInsightsResult
{
    public string Summary { get; set; } = string.Empty;
    public List<InventoryAlert> Alerts { get; set; } = [];
    public List<RestockRecommendation> RestockRecommendations { get; set; } = [];
    public List<SlowMovingProduct> SlowMovingProducts { get; set; } = [];
    public List<FastMovingProduct> FastMovingProducts { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public decimal EstimatedStockoutRisk { get; set; }
}

public class InventoryAlert
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // "Critical", "Warning", "Info"
    public string Message { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int DaysUntilStockout { get; set; }
}

public class RestockRecommendation
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int SuggestedReorderQuantity { get; set; }
    public string Urgency { get; set; } = string.Empty; // "Immediate", "Soon", "Planned"
    public string Reason { get; set; } = string.Empty;
}

public class SlowMovingProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int UnitsSoldLast30Days { get; set; }
    public int DaysInInventory { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class FastMovingProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int UnitsSoldLast30Days { get; set; }
    public decimal SalesVelocity { get; set; } // Units per day
}

#endregion

#region Pricing Recommendations

public class PricingRecommendationsResult
{
    public string Summary { get; set; } = string.Empty;
    public List<PricingRecommendation> Recommendations { get; set; } = [];
    public List<string> KeyInsights { get; set; } = [];
    public decimal PotentialRevenueIncrease { get; set; }
}

public class PricingRecommendation
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal SuggestedPrice { get; set; }
    public decimal PriceChangePercent { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public string ImpactLevel { get; set; } = string.Empty; // "High", "Medium", "Low"
}

#endregion

#region Customer Insights

public class CustomerInsightsResult
{
    public string Summary { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public List<CustomerSegment> Segments { get; set; } = [];
    public List<ChurnRiskCustomer> ChurnRiskCustomers { get; set; } = [];
    public List<HighValueCustomer> HighValueCustomers { get; set; } = [];
    public List<string> KeyInsights { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}

public class CustomerSegment
{
    public string Name { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal AverageSpend { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ChurnRiskCustomer
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DaysSinceLastOrder { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal ChurnProbability { get; set; }
    public string SuggestedAction { get; set; } = string.Empty;
}

public class HighValueCustomer
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

#endregion

#region Review Analysis

public class ReviewAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public string OverallSentiment { get; set; } = string.Empty;
    public List<ProductSentiment> ProductSentiments { get; set; } = [];
    public List<string> CommonPraises { get; set; } = [];
    public List<string> CommonComplaints { get; set; } = [];
    public List<ReviewAlert> ReviewAlerts { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}

public class ProductSentiment
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
    public string Sentiment { get; set; } = string.Empty;
    public string SentimentTrend { get; set; } = string.Empty; // "Improving", "Declining", "Stable"
}

public class ReviewAlert
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // "NegativeTrend", "QualityIssue", "NeedsAttention"
    public string Message { get; set; } = string.Empty;
    public int RecentNegativeCount { get; set; }
}

#endregion

#region Product Description Generation

public class ProductDescriptionRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Type { get; set; }
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public string? ExistingDescription { get; set; }
    public string Tone { get; set; } = "Professional"; // "Professional", "Casual", "Luxury", "Playful"
}

public class ProductDescriptionResult
{
    public string ShortDescription { get; set; } = string.Empty;
    public string LongDescription { get; set; } = string.Empty;
    public List<string> SuggestedKeywords { get; set; } = [];
    public List<string> SuggestedTags { get; set; } = [];
    public string MetaDescription { get; set; } = string.Empty;
}

#endregion
