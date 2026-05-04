using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// Service for tracking user interactions with products (clickstream data).
/// </summary>
public interface IUserInteractionService
{
    Task TrackInteractionAsync(string userId, int productId, InteractionType type, 
        string? sessionId = null, int? durationSeconds = null);
    
    Task<List<UserInteraction>> GetUserInteractionsAsync(string userId, int limit = 100);
    
    Task<List<UserInteraction>> GetProductInteractionsAsync(int productId, int limit = 100);
    
    /// <summary>
    /// Gets the most viewed/interacted products for a user (for personalization).
    /// </summary>
    Task<List<int>> GetUserTopProductsAsync(string userId, int count = 20);
}
