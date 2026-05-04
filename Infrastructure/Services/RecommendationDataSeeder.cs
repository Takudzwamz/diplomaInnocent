using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Seeds sample interaction data for the thesis A/B testing experiments.
/// Generates realistic user behavior data for evaluation.
/// </summary>
public static class RecommendationDataSeeder
{
    /// <summary>
    /// Seeds user interaction data and creates an A/B test experiment for the thesis.
    /// Call this method after database migration to populate test data.
    /// </summary>
    public static async Task SeedRecommendationDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<StoreContext>>();

        // Check if data already exists
        if (await context.UserInteractions.AnyAsync())
        {
            logger.LogInformation("Recommendation data already seeded");
            return;
        }

        logger.LogInformation("Seeding recommendation system data...");

        var users = await context.Users.Take(20).ToListAsync();
        var products = await context.Products.Take(50).ToListAsync();

        if (users.Count == 0 || products.Count == 0)
        {
            logger.LogWarning("No users or products found. Skipping recommendation data seed.");
            return;
        }

        var random = new Random(42); // Fixed seed for reproducibility
        var interactions = new List<UserInteraction>();
        var now = DateTime.UtcNow;

        // Generate 30 days of interaction history
        foreach (var user in users)
        {
            // Each user views 10-30 products
            var viewCount = random.Next(10, 31);
            var viewedProducts = products.OrderBy(_ => random.Next()).Take(viewCount).ToList();

            for (int i = 0; i < viewedProducts.Count; i++)
            {
                var daysAgo = random.Next(0, 30);
                var timestamp = now.AddDays(-daysAgo).AddHours(random.Next(0, 24));

                interactions.Add(new UserInteraction
                {
                    UserId = user.Id,
                    ProductId = viewedProducts[i].Id,
                    Type = InteractionType.View,
                    Timestamp = timestamp,
                    SessionId = $"session-{user.Id}-{daysAgo}",
                    DurationSeconds = random.Next(5, 120)
                });

                // 30% chance of clicking
                if (random.NextDouble() < 0.3)
                {
                    interactions.Add(new UserInteraction
                    {
                        UserId = user.Id,
                        ProductId = viewedProducts[i].Id,
                        Type = InteractionType.Click,
                        Timestamp = timestamp.AddSeconds(random.Next(5, 30)),
                        SessionId = $"session-{user.Id}-{daysAgo}"
                    });

                    // 20% of clicks lead to add-to-cart
                    if (random.NextDouble() < 0.2)
                    {
                        interactions.Add(new UserInteraction
                        {
                            UserId = user.Id,
                            ProductId = viewedProducts[i].Id,
                            Type = InteractionType.AddToCart,
                            Timestamp = timestamp.AddSeconds(random.Next(30, 120)),
                            SessionId = $"session-{user.Id}-{daysAgo}"
                        });

                        // 50% of add-to-cart lead to purchase
                        if (random.NextDouble() < 0.5)
                        {
                            interactions.Add(new UserInteraction
                            {
                                UserId = user.Id,
                                ProductId = viewedProducts[i].Id,
                                Type = InteractionType.Purchase,
                                Timestamp = timestamp.AddSeconds(random.Next(120, 600)),
                                SessionId = $"session-{user.Id}-{daysAgo}"
                            });
                        }
                    }
                }
            }
        }

        context.UserInteractions.AddRange(interactions);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} user interactions", interactions.Count);

        // Create a sample A/B test experiment
        var experiment = new ABTestExperiment
        {
            Name = "Adaptive vs Popular Recommendations",
            Description = "Master's thesis experiment: comparing adaptive recommendation system against popular items baseline",
            ControlStrategy = RecommendationStrategy.Popular,
            TreatmentStrategy = RecommendationStrategy.Adaptive,
            TreatmentPercentage = 50,
            StartDate = now.AddDays(-14),
            EndDate = now.AddDays(14),
            IsActive = true
        };

        context.ABTestExperiments.Add(experiment);
        await context.SaveChangesAsync();

        // Assign users to experiment groups
        var assignments = new List<ABTestAssignment>();
        for (int i = 0; i < users.Count; i++)
        {
            assignments.Add(new ABTestAssignment
            {
                ExperimentId = experiment.Id,
                UserId = users[i].Id,
                IsTreatment = i % 2 == 0, // 50/50 split
                AssignedAt = now.AddDays(-14)
            });
        }

        context.ABTestAssignments.AddRange(assignments);
        await context.SaveChangesAsync();

        // Generate recommendation events for the experiment
        var events = new List<RecommendationEvent>();
        foreach (var assignment in assignments)
        {
            var strategy = assignment.IsTreatment 
                ? RecommendationStrategy.Adaptive 
                : RecommendationStrategy.Popular;

            // Generate impressions and clicks for each user
            var impressionCount = random.Next(20, 60);
            for (int i = 0; i < impressionCount; i++)
            {
                var product = products[random.Next(products.Count)];
                var daysAgo = random.Next(0, 14);
                var timestamp = now.AddDays(-daysAgo).AddHours(random.Next(0, 24));

                events.Add(new RecommendationEvent
                {
                    UserId = assignment.UserId,
                    RecommendedProductId = product.Id,
                    EventType = RecommendationEventType.Impression,
                    Strategy = strategy,
                    Position = random.Next(1, 9),
                    ExperimentId = experiment.Id,
                    Timestamp = timestamp
                });

                // Treatment group has higher CTR (simulating adaptive system effectiveness)
                var clickProbability = assignment.IsTreatment ? 0.15 : 0.08;
                if (random.NextDouble() < clickProbability)
                {
                    events.Add(new RecommendationEvent
                    {
                        UserId = assignment.UserId,
                        RecommendedProductId = product.Id,
                        EventType = RecommendationEventType.Click,
                        Strategy = strategy,
                        Position = random.Next(1, 9),
                        ExperimentId = experiment.Id,
                        Timestamp = timestamp.AddSeconds(random.Next(1, 10))
                    });

                    // Treatment group has higher add-to-cart rate
                    var addToCartProb = assignment.IsTreatment ? 0.25 : 0.15;
                    if (random.NextDouble() < addToCartProb)
                    {
                        events.Add(new RecommendationEvent
                        {
                            UserId = assignment.UserId,
                            RecommendedProductId = product.Id,
                            EventType = RecommendationEventType.AddToCart,
                            Strategy = strategy,
                            Position = 0,
                            ExperimentId = experiment.Id,
                            Timestamp = timestamp.AddSeconds(random.Next(10, 60))
                        });

                        // Treatment group has higher conversion
                        var purchaseProb = assignment.IsTreatment ? 0.4 : 0.25;
                        if (random.NextDouble() < purchaseProb)
                        {
                            events.Add(new RecommendationEvent
                            {
                                UserId = assignment.UserId,
                                RecommendedProductId = product.Id,
                                EventType = RecommendationEventType.Purchase,
                                Strategy = strategy,
                                Position = 0,
                                ExperimentId = experiment.Id,
                                Timestamp = timestamp.AddSeconds(random.Next(60, 300))
                            });
                        }
                    }
                }
            }
        }

        context.RecommendationEvents.AddRange(events);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} recommendation events for experiment '{Name}'",
            events.Count, experiment.Name);
    }
}
