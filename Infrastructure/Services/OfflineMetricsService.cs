using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Offline evaluation service for recommendation quality metrics.
/// Implements train/test split evaluation: historical interactions before the split date
/// are used to generate recommendations, interactions in the test period serve as ground truth.
/// </summary>
public class OfflineMetricsService : IOfflineMetricsService
{
    private readonly StoreContext _context;
    private readonly IAdaptiveRecommendationService _recService;
    private readonly ILogger<OfflineMetricsService> _logger;

    public OfflineMetricsService(
        StoreContext context,
        IAdaptiveRecommendationService recService,
        ILogger<OfflineMetricsService> logger)
    {
        _context = context;
        _recService = recService;
        _logger = logger;
    }

    public async Task<OfflineEvaluationResult> EvaluateAsync(OfflineEvaluationRequest request)
    {
        var totalDays = (request.To - request.From).TotalDays;
        var trainEnd = request.From.AddDays(totalDays * request.TrainTestSplit);

        var strategies = request.Strategies.Count > 0
            ? request.Strategies
            : new List<RecommendationStrategy>
            {
                RecommendationStrategy.Popular,
                RecommendationStrategy.CollaborativeFiltering,
                RecommendationStrategy.ContentBased,
                RecommendationStrategy.Adaptive
            };

        // Get users who have interactions in BOTH train and test periods
        var testInteractions = await _context.UserInteractions
            .Where(i => i.Timestamp >= trainEnd && i.Timestamp <= request.To)
            .Where(i => i.Type == InteractionType.Purchase 
                     || i.Type == InteractionType.AddToCart)
            .ToListAsync();

        var trainUserIds = await _context.UserInteractions
            .Where(i => i.Timestamp >= request.From && i.Timestamp < trainEnd)
            .Select(i => i.UserId)
            .Distinct()
            .ToListAsync();

        // Only evaluate users who have activity in both periods
        var testUserProducts = testInteractions
            .Where(i => trainUserIds.Contains(i.UserId))
            .GroupBy(i => i.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(i => i.ProductId).Distinct().ToHashSet()
            );

        var usersToEvaluate = testUserProducts.Keys.ToList();
        var totalCatalogSize = await _context.Products.CountAsync();

        _logger.LogInformation(
            "Running offline evaluation: {UserCount} users, K={K}, strategies={Strategies}",
            usersToEvaluate.Count, request.K, string.Join(",", strategies));

        var result = new OfflineEvaluationResult
        {
            TrainFrom = request.From,
            TrainTo = trainEnd,
            TestFrom = trainEnd,
            TestTo = request.To,
            K = request.K,
            TotalUsersEvaluated = usersToEvaluate.Count
        };

        foreach (var strategy in strategies)
        {
            var metrics = await EvaluateStrategyAsync(
                strategy, usersToEvaluate, testUserProducts, request.K, totalCatalogSize);
            result.StrategyResults.Add(metrics);
        }

        return result;
    }

    private async Task<StrategyOfflineMetrics> EvaluateStrategyAsync(
        RecommendationStrategy strategy,
        List<string> users,
        Dictionary<string, HashSet<int>> groundTruth,
        int k,
        int totalCatalogSize)
    {
        var precisions = new List<double>();
        var recalls = new List<double>();
        var ndcgs = new List<double>();
        var reciprocalRanks = new List<double>();
        var hits = 0;
        var allRecommendedItems = new HashSet<int>();

        foreach (var userId in users)
        {
            List<Product> recommendations;
            try
            {
                recommendations = await _recService.GetRecommendationsAsync(
                    userId, strategy, sourceProductId: null, count: k);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get recommendations for user {UserId} with strategy {Strategy}",
                    userId, strategy);
                continue;
            }

            var recIds = recommendations.Select(p => p.Id).ToList();
            var relevant = groundTruth[userId];

            // Track all recommended items for coverage calculation
            foreach (var id in recIds)
                allRecommendedItems.Add(id);

            // Precision@K: |recommended ∩ relevant| / K
            var hitsInTopK = recIds.Count(id => relevant.Contains(id));
            var precision = (double)hitsInTopK / k;
            precisions.Add(precision);

            // Recall@K: |recommended ∩ relevant| / |relevant|
            var recall = relevant.Count > 0 ? (double)hitsInTopK / relevant.Count : 0;
            recalls.Add(recall);

            // Hit Rate: at least one relevant item in top-K
            if (hitsInTopK > 0) hits++;

            // NDCG@K
            var dcg = 0.0;
            for (int i = 0; i < recIds.Count; i++)
            {
                if (relevant.Contains(recIds[i]))
                {
                    dcg += 1.0 / Math.Log2(i + 2); // position is 1-indexed: log2(rank + 1)
                }
            }

            // Ideal DCG: all relevant items ranked at the top
            var idealHits = Math.Min(relevant.Count, k);
            var idcg = 0.0;
            for (int i = 0; i < idealHits; i++)
            {
                idcg += 1.0 / Math.Log2(i + 2);
            }

            var ndcg = idcg > 0 ? dcg / idcg : 0;
            ndcgs.Add(ndcg);

            // MRR: 1/rank of first relevant item
            var firstRelevantRank = 0;
            for (int i = 0; i < recIds.Count; i++)
            {
                if (relevant.Contains(recIds[i]))
                {
                    firstRelevantRank = i + 1;
                    break;
                }
            }
            reciprocalRanks.Add(firstRelevantRank > 0 ? 1.0 / firstRelevantRank : 0);
        }

        var evaluated = precisions.Count;
        return new StrategyOfflineMetrics
        {
            Strategy = strategy,
            PrecisionAtK = evaluated > 0 ? precisions.Average() : 0,
            RecallAtK = evaluated > 0 ? recalls.Average() : 0,
            NDCGAtK = evaluated > 0 ? ndcgs.Average() : 0,
            MRR = evaluated > 0 ? reciprocalRanks.Average() : 0,
            HitRateAtK = evaluated > 0 ? (double)hits / evaluated : 0,
            Coverage = totalCatalogSize > 0 ? (double)allRecommendedItems.Count / totalCatalogSize : 0,
            UsersEvaluated = evaluated
        };
    }
}
