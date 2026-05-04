using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// Service for computing offline recommendation quality metrics.
/// Supports thesis Chapter 4: Experimental Evaluation (Precision@K, Recall@K, NDCG@K, Coverage, MRR).
/// </summary>
public interface IOfflineMetricsService
{
    /// <summary>
    /// Runs a full offline evaluation of all strategies over a date range.
    /// Uses a train/test split: interactions before the split date train the model,
    /// interactions after serve as ground truth.
    /// </summary>
    Task<OfflineEvaluationResult> EvaluateAsync(OfflineEvaluationRequest request);
}

public class OfflineEvaluationRequest
{
    /// <summary>Start of the evaluation period.</summary>
    public DateTime From { get; set; }

    /// <summary>End of the evaluation period.</summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Fraction of the period used for training (e.g. 0.8 = 80% train, 20% test).
    /// </summary>
    public double TrainTestSplit { get; set; } = 0.8;

    /// <summary>Top-K cutoff for metrics calculation.</summary>
    public int K { get; set; } = 10;

    /// <summary>
    /// Which strategies to evaluate. If empty, evaluates all.
    /// </summary>
    public List<RecommendationStrategy> Strategies { get; set; } = [];
}

public class OfflineEvaluationResult
{
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public DateTime TrainFrom { get; set; }
    public DateTime TrainTo { get; set; }
    public DateTime TestFrom { get; set; }
    public DateTime TestTo { get; set; }
    public int K { get; set; }
    public int TotalUsersEvaluated { get; set; }
    public List<StrategyOfflineMetrics> StrategyResults { get; set; } = [];
}

public class StrategyOfflineMetrics
{
    public RecommendationStrategy Strategy { get; set; }

    /// <summary>
    /// Precision@K: fraction of recommended items in top-K that are relevant (purchased/interacted in test set).
    /// </summary>
    public double PrecisionAtK { get; set; }

    /// <summary>
    /// Recall@K: fraction of relevant items (from test set) that appear in top-K recommendations.
    /// </summary>
    public double RecallAtK { get; set; }

    /// <summary>
    /// F1@K: harmonic mean of Precision@K and Recall@K.
    /// </summary>
    public double F1AtK => PrecisionAtK + RecallAtK > 0 
        ? 2 * PrecisionAtK * RecallAtK / (PrecisionAtK + RecallAtK) : 0;

    /// <summary>
    /// NDCG@K: Normalized Discounted Cumulative Gain — measures ranking quality.
    /// </summary>
    public double NDCGAtK { get; set; }

    /// <summary>
    /// MRR (Mean Reciprocal Rank): average of 1/rank of first relevant item.
    /// </summary>
    public double MRR { get; set; }

    /// <summary>
    /// Hit Rate@K: fraction of users for whom at least one recommendation was relevant.
    /// </summary>
    public double HitRateAtK { get; set; }

    /// <summary>
    /// Coverage: fraction of total catalog items that appear in recommendations across all users.
    /// </summary>
    public double Coverage { get; set; }

    /// <summary>
    /// Number of users evaluated for this strategy.
    /// </summary>
    public int UsersEvaluated { get; set; }
}
