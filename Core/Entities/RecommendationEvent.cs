namespace Core.Entities;

/// <summary>
/// Tracks when recommendation impressions and clicks occur for CTR calculation.
/// </summary>
public class RecommendationEvent : BaseEntity
{
    public required string UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// ID of the product that was recommended.
    /// </summary>
    public int RecommendedProductId { get; set; }
    public Product RecommendedProduct { get; set; } = null!;

    /// <summary>
    /// The source product from which this recommendation was generated (if applicable).
    /// </summary>
    public int? SourceProductId { get; set; }

    public RecommendationEventType EventType { get; set; }

    /// <summary>
    /// Which strategy generated this recommendation.
    /// </summary>
    public RecommendationStrategy Strategy { get; set; }

    /// <summary>
    /// Position in the recommendation list (1-based).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// A/B test experiment this event belongs to (if any).
    /// </summary>
    public int? ExperimentId { get; set; }
    public ABTestExperiment? Experiment { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum RecommendationEventType
{
    Impression = 0,
    Click = 1,
    AddToCart = 2,
    Purchase = 3
}
