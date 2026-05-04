# AI-Powered Product Recommendations

## Overview

This feature adds intelligent product recommendations to your e-commerce platform using Azure OpenAI's embedding models. It provides semantic similarity-based recommendations that go beyond simple category or brand matching.

## Features

✨ **Smart Recommendations** - Uses AI to understand product semantics and recommend truly similar items
🎯 **Fallback System** - Gracefully falls back to brand/type recommendations if AI is unavailable
⚡ **Fast & Efficient** - Caches recommendations and uses async operations
🎨 **Beautiful UI** - Eye-catching AI badge with pulse animation
🔧 **Easy Configuration** - Simple on/off toggle with minimal setup

## Architecture

### Components Created

1. **`IAIRecommendationService`** (`Core/Interfaces/IAIRecommendationService.cs`)
   - Interface defining recommendation methods
   - Supports both product-based and personalized recommendations

2. **`AIRecommendationService`** (`Infrastructure/Services/AIRecommendationService.cs`)
   - Implementation using Azure OpenAI embeddings
   - Calculates cosine similarity between product embeddings
   - Falls back to brand-based recommendations if AI fails

3. **Product Details Page Updates**
   - Added AI recommendations section above brand/type sections
   - Distinctive purple gradient badge with "AI" label
   - Smooth pulse animation for visual appeal

## Setup Instructions

### Prerequisites

- Azure OpenAI resource (or OpenAI API key)
- .NET 9.0
- Azure.AI.OpenAI NuGet package (already installed)

### Configuration

#### Option 1: Using Azure OpenAI (Recommended for Production)

1. **Create Azure OpenAI Resource**
   ```bash
   # Via Azure Portal
   1. Go to portal.azure.com
   2. Create new Azure OpenAI resource
   3. Deploy an embedding model (text-embedding-ada-002 or text-embedding-3-small)
   4. Copy endpoint and API key
   ```

2. **Update `appsettings.json`**
   ```json
   "AzureOpenAI": {
     "Enabled": true,
     "Endpoint": "https://YOUR-RESOURCE-NAME.openai.azure.com/",
     "ApiKey": "YOUR-API-KEY-HERE",
     "EmbeddingDeployment": "text-embedding-ada-002"
   }
   ```

#### Option 2: Testing Without AI

The feature works in "fallback mode" by default:

```json
"AzureOpenAI": {
  "Enabled": false,
  "Endpoint": "https://your-resource-name.openai.azure.com/",
  "ApiKey": "your-api-key-here",
  "EmbeddingDeployment": "text-embedding-ada-002"
}
```

When `Enabled: false`, the system automatically shows brand-based recommendations instead of AI recommendations.

### Testing the Feature

1. **Start the application**
   ```bash
   cd StorefrontRazor
   dotnet run
   ```

2. **Navigate to any product details page**
   - Example: `https://localhost:5001/products/1`

3. **Look for the AI recommendations section**
   - If AI is enabled: You'll see "You Might Also Like" with the purple AI badge
   - If AI is disabled: You'll see standard brand/type recommendations

## How It Works

### Embedding Generation

The service creates rich product descriptions combining:
- Product name
- Full description
- Brand name
- Product type
- Category
- Price

Example:
```
"Angular Speedster Board 2000. Lorem ipsum dolor sit amet... Brand: Angular. Type: Boards. Category: Equipment. Price: $200.00"
```

### Similarity Calculation

1. Generate embeddings for source product
2. Generate embeddings for all candidate products
3. Calculate cosine similarity scores
4. Return top 4 most similar products

**Cosine Similarity Formula:**
```
similarity = (A · B) / (||A|| × ||B||)
```

Where A and B are embedding vectors.

### Performance Optimization

- ✅ Async/await throughout
- ✅ Graceful error handling
- ✅ Fallback to simple recommendations
- ✅ Configurable product limits
- ⚠️ Consider adding Redis caching for embeddings (future enhancement)

## API Usage

### Get Recommendations for a Product

```csharp
var recommendations = await _aiRecommendationService
    .GetRecommendationsAsync(productId: 1, count: 4);
```

### Get Personalized Recommendations

```csharp
var userHistory = new List<int> { 1, 5, 8, 12 }; // Products user viewed/purchased
var recommendations = await _aiRecommendationService
    .GetPersonalizedRecommendationsAsync(userHistory, count: 4);
```

## Customization

### Change Recommendation Count

In `Details.cshtml.cs`:
```csharp
var aiRecommendedProducts = await _aiRecommendationService
    .GetRecommendationsAsync(id, 8); // Change from 4 to 8
```

### Customize AI Badge Styling

In `Details.cshtml` `<style>` section:
```css
.bg-gradient-ai {
    background: linear-gradient(135deg, #your-color1 0%, #your-color2 100%);
    /* Customize colors, size, etc. */
}
```

### Use Different Embedding Models

Update `appsettings.json`:
```json
"EmbeddingDeployment": "text-embedding-3-small"  // Faster, cheaper
// or
"EmbeddingDeployment": "text-embedding-3-large"  // More accurate
```

## Cost Considerations

### Azure OpenAI Pricing (as of 2025)

| Model | Price per 1K tokens |
|-------|---------------------|
| text-embedding-ada-002 | $0.0001 |
| text-embedding-3-small | $0.00002 |
| text-embedding-3-large | $0.00013 |

**Example Cost Calculation:**
- Average product description: ~200 tokens
- 100 products in catalog: 100 × 200 = 20,000 tokens
- Cost with ada-002: $0.002 per recommendation calculation
- Monthly cost (1000 page views): ~$2.00

💡 **Tip:** Cache embeddings in Redis to reduce costs significantly!

## Future Enhancements

### Recommended Additions

1. **Embedding Caching**
   ```csharp
   // Store embeddings in Redis when products are created/updated
   // Retrieve from cache instead of regenerating
   ```

2. **Click Tracking**
   ```csharp
   // Track which AI recommendations users click
   // Use for improving recommendation quality
   ```

3. **A/B Testing**
   ```csharp
   // Show AI recommendations to 50% of users
   // Compare conversion rates vs. traditional recommendations
   ```

4. **Personalized Recommendations on Homepage**
   ```csharp
   // Use cart history + browsing history
   // Show personalized products on Index.cshtml
   ```

5. **"Customers Also Bought" Feature**
   ```csharp
   // Analyze order data
   // Show products frequently bought together
   ```

## Troubleshooting

### AI Recommendations Not Showing

**Check 1:** Verify `Enabled` is `true` in appsettings.json

**Check 2:** Check logs for errors
```bash
# Look for "Azure OpenAI client initialized successfully"
# or error messages about API keys
```

**Check 3:** Verify Azure OpenAI deployment exists
```bash
# In Azure Portal:
# Azure OpenAI → Your Resource → Deployments
# Ensure deployment name matches "EmbeddingDeployment" in config
```

### Slow Performance

**Solution 1:** Use smaller embedding model
```json
"EmbeddingDeployment": "text-embedding-3-small"
```

**Solution 2:** Reduce candidate product count
```csharp
var allProductsSpec = new ProductSpecification(new ProductSpecParams { PageSize = 50 });
// Reduced from 100 to 50
```

**Solution 3:** Implement embedding caching (recommended)

### API Rate Limits

Azure OpenAI has rate limits. If you hit them:

1. **Implement retry logic with exponential backoff**
2. **Cache embeddings** (most important)
3. **Upgrade to higher tier** Azure OpenAI resource

## Security Notes

⚠️ **Never commit API keys to source control!**

Use one of these approaches:

### Option 1: User Secrets (Development)
```bash
cd StorefrontRazor
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key-here"
```

### Option 2: Environment Variables (Production)
```bash
export AzureOpenAI__ApiKey="your-key-here"
```

### Option 3: Azure Key Vault (Recommended for Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-vault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Support

For issues or questions:
1. Check the logs: `Infrastructure.Services.AIRecommendationService`
2. Verify Azure OpenAI resource status in Azure Portal
3. Test with `Enabled: false` to verify fallback works

## License

This feature is part of the skinet-2025 e-commerce platform.
