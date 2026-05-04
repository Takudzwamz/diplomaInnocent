using Azure;
using Azure.AI.OpenAI;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.ClientModel;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AIRecommendationService : IAIRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AIRecommendationService> _logger;
    private readonly AzureOpenAIClientService _openAIClientService;

    public AIRecommendationService(
        IUnitOfWork unitOfWork,
        AzureOpenAIClientService openAIClientService,
        ILogger<AIRecommendationService> logger)
    {
        _unitOfWork = unitOfWork;
        _openAIClientService = openAIClientService;
        _logger = logger;
    }

    public async Task<List<Product>> GetRecommendationsAsync(int productId, int count = 4)
    {
        // If AI is not enabled, fall back to simple brand/type recommendations
        if (!_openAIClientService.IsEnabled || _openAIClientService.Client is null)
        {
            return await GetFallbackRecommendationsAsync(productId, count);
        }

        try
        {
            // Get the source product with its embedding
            var productSpec = new ProductSpecification(productId, withImages: true);
            var sourceProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpec(productSpec);

            if (sourceProduct == null)
            {
                _logger.LogWarning($"Product {productId} not found");
                return new List<Product>();
            }

            // Check if source product has embedding
            if (string.IsNullOrEmpty(sourceProduct.Embedding))
            {
                _logger.LogWarning($"Product {productId} has no embedding, falling back to simple recommendations");
                return await GetFallbackRecommendationsAsync(productId, count);
            }

            // OPTIMIZATION: Use pre-computed embedding from database (NO API CALL!)
            float[]? sourceEmbedding;
            try
            {
                sourceEmbedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(sourceProduct.Embedding!);
                if (sourceEmbedding == null)
                {
                    return await GetFallbackRecommendationsAsync(productId, count);
                }
            }
            catch
            {
                _logger.LogWarning($"Failed to deserialize embedding for product {productId}");
                return await GetFallbackRecommendationsAsync(productId, count);
            }

            // MAJOR OPTIMIZATION: Load candidate products WITHOUT variants (just need ID, Name, Embedding)
            // This is 10x faster than loading full product data with variants
            var candidateProducts = await _unitOfWork.Repository<Product>().GetQueryable()
                .Where(p => 
                    (p.CategoryId == sourceProduct.CategoryId || p.ProductBrandId == sourceProduct.ProductBrandId) &&
                    p.Id != productId &&
                    p.Embedding != null && p.Embedding != "")
                .Select(p => new 
                { 
                    p.Id, 
                    p.Name, 
                    p.Embedding,
                    p.Price,
                    p.ProductBrandId,
                    p.ProductTypeId,
                    p.CategoryId
                })
                .Take(50)
                .ToListAsync();

            // PARALLEL PROCESSING: Calculate similarity scores in parallel for massive speed boost
            var recommendations = new System.Collections.Concurrent.ConcurrentBag<(int ProductId, double Score)>();

            Parallel.ForEach(candidateProducts, candidate =>
            {
                try
                {
                    var candidateEmbedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(candidate.Embedding!);
                    if (candidateEmbedding != null)
                    {
                        var similarity = CalculateCosineSimilarity(sourceEmbedding, candidateEmbedding);
                        recommendations.Add((candidate.Id, similarity));
                    }
                }
                catch
                {
                    // Skip products with invalid embeddings
                }
            });

            // Get the top N product IDs by similarity
            var topProductIds = recommendations
                .OrderByDescending(r => r.Score)
                .Take(count)
                .Select(r => r.ProductId)
                .ToList();

            // Now load full product details ONLY for the top recommendations
            if (!topProductIds.Any()) return new List<Product>();
            
            var recommendedProducts = await _unitOfWork.Repository<Product>().ListAsync(
                new ProductSpecification(topProductIds));

            // Return recommendations in the order of similarity scores
            return topProductIds
                .Select(id => recommendedProducts.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .ToList()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating AI recommendations for product {productId}");
            // Fall back to simple recommendations
            return await GetFallbackRecommendationsAsync(productId, count);
        }
    }

    public async Task<List<Product>> GetPersonalizedRecommendationsAsync(List<int> productIds, int count = 4)
    {
        if (!_openAIClientService.IsEnabled || _openAIClientService.Client is null || !productIds.Any())
        {
            _logger.LogInformation("AI disabled or no product IDs provided for personalized recommendations");
            return new List<Product>(); // Return empty list instead of fallback
        }

        try
        {
            // OPTIMIZATION: Get pre-computed embeddings from database (no API calls!)
            var userProducts = await _unitOfWork.Repository<Product>().ListAsync(
                new ProductSpecification(productIds.Take(10).ToList())); // Increased from 5 to 10
            
            var userEmbeddings = new List<float[]>();

            foreach (var product in userProducts.Where(p => !string.IsNullOrEmpty(p.Embedding)))
            {
                try
                {
                    var embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(product.Embedding!);
                    if (embedding != null)
                    {
                        userEmbeddings.Add(embedding);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to deserialize embedding for product {product.Id}");
                    continue;
                }
            }

            if (!userEmbeddings.Any())
            {
                _logger.LogWarning("No valid embeddings found for user's purchase history");
                return new List<Product>(); // Return empty instead of fallback
            }

            _logger.LogInformation($"Found {userEmbeddings.Count} embeddings for personalized recommendations");

            // Calculate average embedding (user profile) - using simple arrays now
            var avgEmbedding = AverageEmbeddingsFromArrays(userEmbeddings);
            
            // MAJOR OPTIMIZATION: Load candidate products WITHOUT variants (just need ID and Embedding)
            var candidateProducts = await _unitOfWork.Repository<Product>().GetQueryable()
                .Where(p => !productIds.Contains(p.Id) && p.Embedding != null && p.Embedding != "")
                .Select(p => new { p.Id, p.Embedding })
                .Take(100)
                .ToListAsync();

            _logger.LogInformation($"Found {candidateProducts.Count} candidate products with embeddings");

            // PARALLEL PROCESSING: Calculate similarity scores in parallel
            var recommendations = new System.Collections.Concurrent.ConcurrentBag<(int ProductId, double Score)>();

            Parallel.ForEach(candidateProducts, candidate =>
            {
                try
                {
                    var productEmbedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(candidate.Embedding!);
                    if (productEmbedding != null)
                    {
                        var similarity = CalculateCosineSimilarity(avgEmbedding, productEmbedding);
                        recommendations.Add((candidate.Id, similarity));
                    }
                }
                catch
                {
                    // Skip products with invalid embeddings
                }
            });

            // Get top N product IDs
            var topProductIds = recommendations
                .OrderByDescending(r => r.Score)
                .Take(count)
                .Select(r => r.ProductId)
                .ToList();

            if (!topProductIds.Any())
            {
                _logger.LogWarning("No recommendations found after similarity calculation");
                return new List<Product>();
            }

            // Load full product details ONLY for top recommendations
            var topRecommendations = await _unitOfWork.Repository<Product>().ListAsync(
                new ProductSpecification(topProductIds));

            _logger.LogInformation($"Returning {topRecommendations.Count} personalized recommendations");

            // Return in order of similarity scores
            return topProductIds
                .Select(id => topRecommendations.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .ToList()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personalized recommendations");
            return new List<Product>(); // Return empty list on error
        }
    }

    private async Task<List<Product>> GetFallbackRecommendationsAsync(int productId, int count)
    {
        // Simple fallback: return products from same brand or type
        var productSpec = new ProductSpecification(productId, withImages: true);
        var sourceProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpec(productSpec);

        if (sourceProduct == null) return new List<Product>();

        var brandSpec = new ProductSpecification(new ProductSpecParams
        {
            BrandId = sourceProduct.ProductBrandId,
            PageSize = count * 2
        });

        var similarProducts = await _unitOfWork.Repository<Product>().ListAsync(brandSpec);

        return [.. similarProducts
            .Where(p => p.Id != productId)
            .Take(count)];
    }

    private string CreateProductDescription(Product product)
    {
        // Create a rich description for embedding
        return $"{product.Name}. {product.Description}. " +
               $"Brand: {product.ProductBrand?.Name ?? "Unknown"}. Type: {product.ProductType?.Name ?? "Unknown"}. " +
               $"Category: {product.Category?.Name ?? "Unknown"}. Price: {product.Price:C}";
    }

    private double CalculateCosineSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length)
        {
            throw new ArgumentException("Embeddings must have the same length");
        }

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            magnitude1 += embedding1[i] * embedding1[i];
            magnitude2 += embedding2[i] * embedding2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
        {
            return 0;
        }

        return dotProduct / (magnitude1 * magnitude2);
    }

    private float[] AverageEmbeddings(List<ReadOnlyMemory<float>> embeddings)
    {
        if (!embeddings.Any()) return Array.Empty<float>();

        var length = embeddings[0].Length;
        var avgEmbedding = new float[length];

        foreach (var embedding in embeddings)
        {
            var array = embedding.ToArray();
            for (int i = 0; i < length; i++)
            {
                avgEmbedding[i] += array[i];
            }
        }

        for (int i = 0; i < length; i++)
        {
            avgEmbedding[i] /= embeddings.Count;
        }

        return avgEmbedding;
    }

    private float[] AverageEmbeddingsFromArrays(List<float[]> embeddings)
    {
        if (!embeddings.Any()) return Array.Empty<float>();

        var length = embeddings[0].Length;
        var avgEmbedding = new float[length];

        foreach (var embedding in embeddings)
        {
            for (int i = 0; i < length; i++)
            {
                avgEmbedding[i] += embedding[i];
            }
        }

        for (int i = 0; i < length; i++)
        {
            avgEmbedding[i] /= embeddings.Count;
        }

        return avgEmbedding;
    }
}
