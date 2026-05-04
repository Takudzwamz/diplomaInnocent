namespace Core.Entities;

/// <summary>
/// Tracks user interactions with products for the recommendation system.
/// Stores clickstream data: views, clicks, add-to-cart, purchases.
/// </summary>
public class UserInteraction : BaseEntity
{
    public required string UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public InteractionType Type { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session identifier for grouping interactions within a single visit.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Duration in seconds (e.g., time spent viewing a product page).
    /// </summary>
    public int? DurationSeconds { get; set; }
}

public enum InteractionType
{
    View = 0,
    Click = 1,
    AddToCart = 2,
    Purchase = 3,
    Wishlist = 4,
    Search = 5,
    RecommendationClick = 6
}
