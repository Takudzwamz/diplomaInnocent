namespace Core.Interfaces;

/// <summary>
/// Service for managing product embeddings used in AI-powered recommendations.
/// Pre-computes and caches embeddings to avoid expensive API calls on every request.
/// </summary>
public interface IProductEmbeddingService
{
    /// <summary>
    /// Generate and store embeddings for products that don't have them yet.
    /// This should be called periodically or when new products are added.
    /// </summary>
    Task GenerateMissingEmbeddingsAsync();
    
    /// <summary>
    /// Get embedding for a specific product (from database cache).
    /// Returns null if embedding doesn't exist.
    /// </summary>
    Task<float[]?> GetProductEmbeddingAsync(int productId);
    
    /// <summary>
    /// Regenerate embedding for a specific product (e.g., after product details change).
    /// </summary>
    Task RegenerateProductEmbeddingAsync(int productId);
}
