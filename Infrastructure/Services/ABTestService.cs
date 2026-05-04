using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ABTestService : IABTestService
{
    private readonly StoreContext _context;
    private readonly ILogger<ABTestService> _logger;

    public ABTestService(StoreContext context, ILogger<ABTestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ABTestExperiment?> GetActiveExperimentAsync()
    {
        return await _context.ABTestExperiments
            .Where(e => e.IsActive && e.StartDate <= DateTime.UtcNow &&
                       (e.EndDate == null || e.EndDate > DateTime.UtcNow))
            .OrderByDescending(e => e.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ABTestAssignment> GetOrAssignUserAsync(string userId, int experimentId)
    {
        // Check if user is already assigned
        var existing = await _context.ABTestAssignments
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ExperimentId == experimentId);

        if (existing != null)
            return existing;

        // Deterministic assignment using hash of userId + experimentId
        var experiment = await _context.ABTestExperiments.FindAsync(experimentId);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        // Use consistent hashing for deterministic assignment
        var hash = GetDeterministicHash(userId, experimentId);
        var isTreatment = (hash % 100) < experiment.TreatmentPercentage;

        var assignment = new ABTestAssignment
        {
            ExperimentId = experimentId,
            UserId = userId,
            IsTreatment = isTreatment,
            AssignedAt = DateTime.UtcNow
        };

        _context.ABTestAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned user {UserId} to experiment {ExperimentId} as {Group}",
            userId, experimentId, isTreatment ? "Treatment" : "Control");

        return assignment;
    }

    public async Task<RecommendationStrategy> GetUserStrategyAsync(string userId)
    {
        var experiment = await GetActiveExperimentAsync();
        if (experiment == null)
        {
            // No active experiment: use adaptive by default
            return RecommendationStrategy.Adaptive;
        }

        var assignment = await GetOrAssignUserAsync(userId, experiment.Id);
        return assignment.IsTreatment ? experiment.TreatmentStrategy : experiment.ControlStrategy;
    }

    public async Task<ABTestExperiment> CreateExperimentAsync(string name, string? description,
        RecommendationStrategy controlStrategy, RecommendationStrategy treatmentStrategy,
        int treatmentPercentage, DateTime startDate, DateTime? endDate)
    {
        // Deactivate any currently active experiments
        var activeExperiments = await _context.ABTestExperiments
            .Where(e => e.IsActive)
            .ToListAsync();

        foreach (var exp in activeExperiments)
        {
            exp.IsActive = false;
            exp.EndDate = DateTime.UtcNow;
        }

        var experiment = new ABTestExperiment
        {
            Name = name,
            Description = description,
            ControlStrategy = controlStrategy,
            TreatmentStrategy = treatmentStrategy,
            TreatmentPercentage = treatmentPercentage,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };

        _context.ABTestExperiments.Add(experiment);
        await _context.SaveChangesAsync();

        return experiment;
    }

    public async Task EndExperimentAsync(int experimentId)
    {
        var experiment = await _context.ABTestExperiments.FindAsync(experimentId);
        if (experiment == null) return;

        experiment.IsActive = false;
        experiment.EndDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deterministic hash for consistent user assignment.
    /// Same user always gets the same group for the same experiment.
    /// </summary>
    private static int GetDeterministicHash(string userId, int experimentId)
    {
        var combined = $"{userId}:{experimentId}";
        unchecked
        {
            int hash = 17;
            foreach (char c in combined)
            {
                hash = hash * 31 + c;
            }
            return Math.Abs(hash);
        }
    }
}
