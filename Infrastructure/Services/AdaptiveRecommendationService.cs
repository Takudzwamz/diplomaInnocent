using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Adaptive recommendation engine implementing multiple strategies for the thesis.
/// Supports: Popular, Collaborative Filtering, Content-Based, and Hybrid Adaptive.
/// </summary>
public class AdaptiveRecommendationService : IAdaptiveRecommendationService
{
    private readonly StoreContext _context;
    private readonly IAIRecommendationService _aiRecommendationService;
    private readonly IUserInteractionService _interactionService;
    private readonly ILogger<AdaptiveRecommendationService> _logger;

    public AdaptiveRecommendationService(
        StoreContext context,
        IAIRecommendationService aiRecommendationService,
        IUserInteractionService interactionService,
        ILogger<AdaptiveRecommendationService> logger)
    {
        _context = context;
        _aiRecommendationService = aiRecommendationService;
        _interactionService = interactionService;
        _logger = logger;
    }

    public async Task<List<Product>> GetRecommendationsAsync(string userId, 
        RecommendationStrategy strategy, int? sourceProductId = null, int count = 8)
    {
        return strategy switch
        {
            RecommendationStrategy.None => new List<Product>(),
            RecommendationStrategy.Popular => await GetPopularProductsAsync(count),
            RecommendationStrategy.CollaborativeFiltering => await GetCollaborativeRecommendationsAsync(userId, count),
            RecommendationStrategy.ContentBased => sourceProductId.HasValue
                ? await GetContentBasedRecommendationsAsync(sourceProductId.Value, count)
                : await GetPopularProductsAsync(count),
            RecommendationStrategy.Adaptive => await GetAdaptiveRecommendationsAsync(userId, count),
            _ => await GetPopularProductsAsync(count)
        };
    }

    /// <summary>
    /// Hybrid adaptive: combines collaborative filtering + content-based + trending.
    /// Weights are adjusted based on data availability (cold-start handling).
    /// </summary>
    public async Task<List<Product>> GetAdaptiveRecommendationsAsync(string userId, int count = 8)
    {
        var userProductIds = await _interactionService.GetUserTopProductsAsync(userId, 20);
        
        // Cold-start: if user has no history, fall back to popular
        if (userProductIds.Count == 0)
        {
            _logger.LogInformation("Cold-start for user {UserId}, using popular recommendations", userId);
            return await GetPopularProductsAsync(count);
        }

        var scoredProducts = new Dictionary<int, double>();

        // Component 1: Collaborative Filtering (weight: 0.4)
        try
        {
            var cfProducts = await GetCollaborativeRecommendationsInternalAsync(userId, count * 2);
            foreach (var (productId, score) in cfProducts)
            {
                scoredProducts.TryAdd(productId, 0);
                scoredProducts[productId] += score * 0.4;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Collaborative filtering failed, continuing with other components");
        }

        // Component 2: Content-Based (weight: 0.35)
        try
        {
            var topProductId = userProductIds.First();
            var contentProducts = await _aiRecommendationService.GetRecommendationsAsync(topProductId, count * 2);
            for (int i = 0; i < contentProducts.Count; i++)
            {
                var productId = contentProducts[i].Id;
                var score = (double)(contentProducts.Count - i) / contentProducts.Count; // position-based score
                scoredProducts.TryAdd(productId, 0);
                scoredProducts[productId] += score * 0.35;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Content-based recommendations failed");
        }

        // Component 3: Trending/Popular (weight: 0.15)
        try
        {
            var trendingProducts = await GetTrendingProductIdsAsync(count * 2);
            for (int i = 0; i < trendingProducts.Count; i++)
            {
                var productId = trendingProducts[i];
                var score = (double)(trendingProducts.Count - i) / trendingProducts.Count;
                scoredProducts.TryAdd(productId, 0);
                scoredProducts[productId] += score * 0.15;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Trending recommendations failed");
        }

        // Component 4: Recency boost (weight: 0.1) - boost recently interacted categories
        try
        {
            var recentCategories = await GetUserRecentCategoriesAsync(userId);
            var categoryProducts = await _context.Products
                .Where(p => recentCategories.Contains(p.CategoryId) && !userProductIds.Contains(p.Id))
                .OrderByDescending(p => p.Id) // newest first
                .Take(count * 2)
                .Select(p => p.Id)
                .ToListAsync();

            for (int i = 0; i < categoryProducts.Count; i++)
            {
                var productId = categoryProducts[i];
                var score = (double)(categoryProducts.Count - i) / categoryProducts.Count;
                scoredProducts.TryAdd(productId, 0);
                scoredProducts[productId] += score * 0.1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Category-based recommendations failed");
        }

        // Remove products user already interacted with
        foreach (var pid in userProductIds)
        {
            scoredProducts.Remove(pid);
        }

        // Get top scored products
        var topProductIds = scoredProducts
            .OrderByDescending(kv => kv.Value)
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();

        if (topProductIds.Count == 0)
        {
            return await GetPopularProductsAsync(count);
        }

        // Load full product data
        var products = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductType)
            .Where(p => topProductIds.Contains(p.Id))
            .ToListAsync();

        // Return in scored order
        return topProductIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList()!;
    }

    public async Task<List<Product>> GetPopularProductsAsync(int count = 8)
    {
        // Popular = most purchased in last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var popularProductIds = await _context.UserInteractions
            .Where(i => i.Type == InteractionType.Purchase && i.Timestamp >= thirtyDaysAgo)
            .GroupBy(i => i.ProductId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();

        // Fallback: if no recent purchase data, use views
        if (popularProductIds.Count < count)
        {
            var viewedIds = await _context.UserInteractions
                .Where(i => i.Timestamp >= thirtyDaysAgo)
                .GroupBy(i => i.ProductId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            popularProductIds = popularProductIds.Union(viewedIds).Take(count).ToList();
        }

        // Final fallback: newest products
        if (popularProductIds.Count < count)
        {
            var newestIds = await _context.Products
                .OrderByDescending(p => p.Id)
                .Take(count)
                .Select(p => p.Id)
                .ToListAsync();

            popularProductIds = popularProductIds.Union(newestIds).Take(count).ToList();
        }

        var products = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductType)
            .Where(p => popularProductIds.Contains(p.Id))
            .ToListAsync();

        return popularProductIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList()!;
    }

    /// <summary>
    /// Collaborative filtering: find users with similar interaction patterns,
    /// recommend products those users liked that the target user hasn't seen.
    /// </summary>
    public async Task<List<Product>> GetCollaborativeRecommendationsAsync(string userId, int count = 8)
    {
        var scored = await GetCollaborativeRecommendationsInternalAsync(userId, count);
        var topIds = scored.OrderByDescending(x => x.Score).Take(count).Select(x => x.ProductId).ToList();

        if (topIds.Count == 0)
        {
            return await GetPopularProductsAsync(count);
        }

        var products = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductType)
            .Where(p => topIds.Contains(p.Id))
            .ToListAsync();

        return topIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList()!;
    }

    public async Task<List<Product>> GetContentBasedRecommendationsAsync(int productId, int count = 8)
    {
        return await _aiRecommendationService.GetRecommendationsAsync(productId, count);
    }

    #region Private Methods

    private async Task<List<(int ProductId, double Score)>> GetCollaborativeRecommendationsInternalAsync(
        string userId, int count)
    {
        // Get the target user's interactions
        var userProducts = await _context.UserInteractions
            .Where(i => i.UserId == userId)
            .Select(i => i.ProductId)
            .Distinct()
            .ToListAsync();

        if (userProducts.Count == 0)
            return new List<(int, double)>();

        // Find similar users: users who interacted with the same products
        var similarUsers = await _context.UserInteractions
            .Where(i => userProducts.Contains(i.ProductId) && i.UserId != userId)
            .GroupBy(i => i.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                CommonProducts = g.Select(i => i.ProductId).Distinct().Count()
            })
            .OrderByDescending(x => x.CommonProducts)
            .Take(20) // Top 20 similar users
            .ToListAsync();

        if (similarUsers.Count == 0)
            return new List<(int, double)>();

        var similarUserIds = similarUsers.Select(u => u.UserId).ToList();

        // Get products that similar users liked but target user hasn't seen
        var recommendations = await _context.UserInteractions
            .Where(i => similarUserIds.Contains(i.UserId) &&
                       !userProducts.Contains(i.ProductId) &&
                       (i.Type == InteractionType.Purchase || i.Type == InteractionType.AddToCart))
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Score = (double)g.Count() / similarUsers.Count // normalized by number of similar users
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .ToListAsync();

        return recommendations.Select(r => (r.ProductId, r.Score)).ToList();
    }

    private async Task<List<int>> GetTrendingProductIdsAsync(int count)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        return await _context.UserInteractions
            .Where(i => i.Timestamp >= sevenDaysAgo)
            .GroupBy(i => i.ProductId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();
    }

    private async Task<List<int>> GetUserRecentCategoriesAsync(string userId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        return await _context.UserInteractions
            .Where(i => i.UserId == userId && i.Timestamp >= sevenDaysAgo)
            .Join(_context.Products,
                interaction => interaction.ProductId,
                product => product.Id,
                (interaction, product) => product.CategoryId)
            .Distinct()
            .ToListAsync();
    }

    #endregion
}
