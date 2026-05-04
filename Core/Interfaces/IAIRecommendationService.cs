using Core.Entities;

namespace Core.Interfaces;

public interface IAIRecommendationService
{
    /// <summary>
    /// Gets AI-powered product recommendations based on semantic similarity
    /// </summary>
    /// <param name="productId">The product to get recommendations for</param>
    /// <param name="count">Number of recommendations to return (default: 4)</param>
    /// <returns>List of recommended products</returns>
    Task<List<Product>> GetRecommendationsAsync(int productId, int count = 4);

    /// <summary>
    /// Gets personalized product recommendations based on user's cart/browsing history
    /// </summary>
    /// <param name="productIds">List of product IDs the user has viewed or added to cart</param>
    /// <param name="count">Number of recommendations to return</param>
    /// <returns>List of recommended products</returns>
    Task<List<Product>> GetPersonalizedRecommendationsAsync(List<int> productIds, int count = 4);
}
