namespace Core.Entities;

/// <summary>
/// A/B test experiment definition.
/// </summary>
public class ABTestExperiment : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// The recommendation strategy used for the control group.
    /// </summary>
    public RecommendationStrategy ControlStrategy { get; set; } = RecommendationStrategy.Popular;

    /// <summary>
    /// The recommendation strategy used for the treatment group.
    /// </summary>
    public RecommendationStrategy TreatmentStrategy { get; set; } = RecommendationStrategy.Adaptive;

    /// <summary>
    /// Percentage of users assigned to the treatment group (0-100).
    /// </summary>
    public int TreatmentPercentage { get; set; } = 50;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<ABTestAssignment> Assignments { get; set; } = [];
}

/// <summary>
/// Assigns a user to a specific A/B test group.
/// </summary>
public class ABTestAssignment : BaseEntity
{
    public int ExperimentId { get; set; }
    public ABTestExperiment Experiment { get; set; } = null!;

    public required string UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Which group: Control (false) or Treatment (true).
    /// </summary>
    public bool IsTreatment { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public enum RecommendationStrategy
{
    /// <summary>No recommendations shown.</summary>
    None = 0,
    /// <summary>Most popular products.</summary>
    Popular = 1,
    /// <summary>Collaborative filtering (user-based).</summary>
    CollaborativeFiltering = 2,
    /// <summary>Content-based (embeddings/cosine similarity).</summary>
    ContentBased = 3,
    /// <summary>Hybrid adaptive system (the thesis model).</summary>
    Adaptive = 4
}
