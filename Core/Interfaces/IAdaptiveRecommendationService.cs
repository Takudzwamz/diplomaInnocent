using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// Adaptive recommendation engine supporting multiple strategies:
/// popular, collaborative filtering, content-based, and hybrid adaptive.
/// </summary>
public interface IAdaptiveRecommendationService
{
    /// <summary>
    /// Gets recommendations using the specified strategy.
    /// </summary>
    Task<List<Product>> GetRecommendationsAsync(string userId, RecommendationStrategy strategy, 
        int? sourceProductId = null, int count = 8);

    /// <summary>
    /// Gets personalized adaptive recommendations (the thesis hybrid model).
    /// Combines collaborative filtering + content-based + user behavior.
    /// </summary>
    Task<List<Product>> GetAdaptiveRecommendationsAsync(string userId, int count = 8);

    /// <summary>
    /// Gets popular products (baseline for A/B testing).
    /// </summary>
    Task<List<Product>> GetPopularProductsAsync(int count = 8);

    /// <summary>
    /// Gets collaborative filtering recommendations based on similar users' behavior.
    /// </summary>
    Task<List<Product>> GetCollaborativeRecommendationsAsync(string userId, int count = 8);

    /// <summary>
    /// Gets content-based recommendations for a specific product.
    /// </summary>
    Task<List<Product>> GetContentBasedRecommendationsAsync(int productId, int count = 8);
}
