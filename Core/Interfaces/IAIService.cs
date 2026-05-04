using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// Interface for AI-powered features including review summarization and chat assistance
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generates an AI-powered summary of product reviews
    /// </summary>
    /// <param name="reviews">List of product reviews to summarize</param>
    /// <returns>Summary with pros, cons, and overall sentiment</returns>
    Task<ReviewSummary?> SummarizeReviewsAsync(List<ProductReview> reviews);

    /// <summary>
    /// Processes a customer question about products and returns an AI-generated response
    /// </summary>
    /// <param name="question">The customer's question</param>
    /// <param name="context">Optional context (e.g., product details, cart contents)</param>
    /// <returns>AI-generated response</returns>
    Task<ChatResponse> ChatAsync(string question, ChatContext? context = null);

    /// <summary>
    /// Performs semantic search across products using natural language
    /// </summary>
    /// <param name="query">Natural language search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <returns>List of matching products with relevance scores</returns>
    Task<List<SearchResult>> SemanticSearchAsync(string query, int maxResults = 20);

    /// <summary>
    /// Checks if the AI service is enabled and configured
    /// </summary>
    bool IsEnabled { get; }
}

/// <summary>
/// AI-generated summary of product reviews
/// </summary>
public class ReviewSummary
{
    /// <summary>
    /// Overall summary paragraph
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// List of positive aspects mentioned by customers
    /// </summary>
    public List<string> Pros { get; set; } = [];

    /// <summary>
    /// List of negative aspects or concerns mentioned
    /// </summary>
    public List<string> Cons { get; set; } = [];

    /// <summary>
    /// Overall sentiment: Positive, Mixed, or Negative
    /// </summary>
    public string Sentiment { get; set; } = "Neutral";

    /// <summary>
    /// Confidence score (0-1)
    /// </summary>
    public double Confidence { get; set; }
}

/// <summary>
/// Response from the AI chat assistant
/// </summary>
public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<int>? SuggestedProductIds { get; set; }
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}

/// <summary>
/// Context provided to the AI chat for better responses
/// </summary>
public class ChatContext
{
    public Product? CurrentProduct { get; set; }
    public List<Product>? CartProducts { get; set; }
    public List<Product>? RecentlyViewed { get; set; }
    public string? UserPreferences { get; set; }
    
    // Extended cart info
    public List<CartContextItem>? CartItems { get; set; }
    public decimal CartTotal { get; set; }
    
    // Order history
    public bool IsLoggedIn { get; set; }
    public List<OrderContextItem>? OrderHistory { get; set; }
}

/// <summary>
/// Cart item for AI context
/// </summary>
public class CartContextItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Order item for AI context
/// </summary>
public class OrderContextItem
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemContext> Items { get; set; } = new();
}

/// <summary>
/// Order line item for AI context
/// </summary>
public class OrderItemContext
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Search result with relevance score
/// </summary>
public class SearchResult
{
    public Product Product { get; set; } = null!;
    public double RelevanceScore { get; set; }
}
