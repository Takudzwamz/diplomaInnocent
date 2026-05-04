using System.Text.Json;
using Azure.AI.OpenAI;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ProductEmbeddingService : IProductEmbeddingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AzureOpenAIClientService _openAIClientService;
    private readonly ILogger<ProductEmbeddingService> _logger;

    public ProductEmbeddingService(
        IUnitOfWork unitOfWork,
        AzureOpenAIClientService openAIClientService,
        ILogger<ProductEmbeddingService> logger)
    {
        _unitOfWork = unitOfWork;
        _openAIClientService = openAIClientService;
        _logger = logger;
    }

    public async Task GenerateMissingEmbeddingsAsync()
    {
        if (!_openAIClientService.IsEnabled || _openAIClientService.Client is null)
        {
            _logger.LogWarning("AI is disabled or not configured. Skipping embedding generation.");
            return;
        }

        try
        {
            // Find products without embeddings
            var allProductsSpec = new ProductSpecification(new ProductSpecParams { PageSize = 1000 });
            var allProducts = await _unitOfWork.Repository<Product>().ListAsync(allProductsSpec);
            var productsWithoutEmbeddings = allProducts.Where(p => string.IsNullOrEmpty(p.Embedding)).ToList();

            if (!productsWithoutEmbeddings.Any())
            {
                _logger.LogInformation("All products already have embeddings.");
                return;
            }

            _logger.LogInformation($"Generating embeddings for {productsWithoutEmbeddings.Count} products...");

            var embeddingClient = _openAIClientService.Client.GetEmbeddingClient(_openAIClientService.EmbeddingDeployment);
            var repo = _unitOfWork.Repository<Product>();

            foreach (var product in productsWithoutEmbeddings)
            {
                try
                {
                    var description = CreateProductDescription(product);
                    var embedding = await embeddingClient.GenerateEmbeddingAsync(description);
                    var embeddingArray = embedding.Value.ToFloats().ToArray();
                    
                    // Store as JSON array
                    product.Embedding = JsonSerializer.Serialize(embeddingArray);
                    repo.Update(product);
                    
                    _logger.LogDebug($"Generated embedding for product {product.Id}: {product.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate embedding for product {product.Id}");
                }
            }

            await _unitOfWork.Complete();
            _logger.LogInformation($"Successfully generated embeddings for {productsWithoutEmbeddings.Count} products.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product embeddings");
        }
    }

    public async Task<float[]?> GetProductEmbeddingAsync(int productId)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product?.Embedding == null) return null;

        try
        {
            return JsonSerializer.Deserialize<float[]>(product.Embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to deserialize embedding for product {productId}");
            return null;
        }
    }

    public async Task RegenerateProductEmbeddingAsync(int productId)
    {
        if (!_openAIClientService.IsEnabled || _openAIClientService.Client is null)
        {
            _logger.LogWarning("AI is disabled. Cannot regenerate embedding.");
            return;
        }

        var productSpec = new ProductSpecification(productId, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(productSpec);
        
        if (product == null)
        {
            _logger.LogWarning($"Product {productId} not found");
            return;
        }

        try
        {
            var embeddingClient = _openAIClientService.Client.GetEmbeddingClient(_openAIClientService.EmbeddingDeployment);
            var description = CreateProductDescription(product);
            var embedding = await embeddingClient.GenerateEmbeddingAsync(description);
            var embeddingArray = embedding.Value.ToFloats().ToArray();
            
            product.Embedding = JsonSerializer.Serialize(embeddingArray);
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.Complete();
            
            _logger.LogInformation($"Regenerated embedding for product {productId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to regenerate embedding for product {productId}");
        }
    }

    private string CreateProductDescription(Product product)
    {
        return $"{product.Name} {product.Description} " +
               $"Brand: {product.ProductBrand?.Name} " +
               $"Type: {product.ProductType?.Name} " +
               $"Category: {product.Category?.Name} " +
               $"Price: {product.Price:C}";
    }
}
