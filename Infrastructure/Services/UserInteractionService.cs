using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class UserInteractionService : IUserInteractionService
{
    private readonly StoreContext _context;
    private readonly ILogger<UserInteractionService> _logger;

    public UserInteractionService(StoreContext context, ILogger<UserInteractionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TrackInteractionAsync(string userId, int productId, InteractionType type,
        string? sessionId = null, int? durationSeconds = null)
    {
        var interaction = new UserInteraction
        {
            UserId = userId,
            ProductId = productId,
            Type = type,
            SessionId = sessionId,
            DurationSeconds = durationSeconds,
            Timestamp = DateTime.UtcNow
        };

        _context.UserInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Tracked {Type} interaction for user {UserId} on product {ProductId}",
            type, userId, productId);
    }

    public async Task<List<UserInteraction>> GetUserInteractionsAsync(string userId, int limit = 100)
    {
        return await _context.UserInteractions
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<UserInteraction>> GetProductInteractionsAsync(int productId, int limit = 100)
    {
        return await _context.UserInteractions
            .Where(i => i.ProductId == productId)
            .OrderByDescending(i => i.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserTopProductsAsync(string userId, int count = 20)
    {
        // Get all interactions and compute scores in memory
        var interactions = await _context.UserInteractions
            .Where(i => i.UserId == userId)
            .Select(i => new { i.ProductId, i.Type })
            .ToListAsync();

        return interactions
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Score = g.Sum(i => i.Type switch
                {
                    InteractionType.Purchase => 5,
                    InteractionType.AddToCart => 3,
                    InteractionType.Click => 2,
                    InteractionType.RecommendationClick => 2,
                    InteractionType.Wishlist => 2,
                    _ => 1
                })
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.ProductId)
            .ToList();
    }
}
