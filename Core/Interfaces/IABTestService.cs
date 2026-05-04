using Core.Entities;

namespace Core.Interfaces;

/// <summary>
/// A/B testing service for comparing recommendation strategies.
/// </summary>
public interface IABTestService
{
    /// <summary>
    /// Gets the active experiment (if any).
    /// </summary>
    Task<ABTestExperiment?> GetActiveExperimentAsync();

    /// <summary>
    /// Assigns a user to an experiment group (or retrieves existing assignment).
    /// Uses deterministic hashing for consistent assignment.
    /// </summary>
    Task<ABTestAssignment> GetOrAssignUserAsync(string userId, int experimentId);

    /// <summary>
    /// Gets the recommendation strategy for a user based on their A/B test assignment.
    /// </summary>
    Task<RecommendationStrategy> GetUserStrategyAsync(string userId);

    /// <summary>
    /// Creates a new A/B test experiment.
    /// </summary>
    Task<ABTestExperiment> CreateExperimentAsync(string name, string? description,
        RecommendationStrategy controlStrategy, RecommendationStrategy treatmentStrategy,
        int treatmentPercentage, DateTime startDate, DateTime? endDate);

    /// <summary>
    /// Ends an active experiment.
    /// </summary>
    Task EndExperimentAsync(int experimentId);
}
