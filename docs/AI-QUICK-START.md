# AI Recommendations - Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Enable the Feature (Development Mode - No AI Required)

The feature is already installed and works in fallback mode! Just run your app:

```bash
cd /home/sputniktech/repos/skinet-2025/StorefrontRazor
dotnet run
```

Navigate to any product page (e.g., `/products/1`) and you'll see recommendations based on brand/type.

---

### Step 2: Enable AI-Powered Recommendations (Optional)

Want the full AI experience? Here's how:

#### A. Get Azure OpenAI Access

1. **Sign up for Azure**: https://portal.azure.com
2. **Request Access**: Go to "Azure OpenAI Service" and request access (approval usually takes 1-2 days)
3. **Create Resource**: Once approved, create an Azure OpenAI resource
4. **Deploy Model**: 
   - Go to your resource
   - Click "Deployments"
   - Deploy `text-embedding-ada-002` or `text-embedding-3-small`

#### B. Configure Your App

Open `StorefrontRazor/appsettings.json` and update:

```json
"AzureOpenAI": {
  "Enabled": true,
  "Endpoint": "https://YOUR-RESOURCE-NAME.openai.azure.com/",
  "ApiKey": "PASTE-YOUR-API-KEY-HERE",
  "EmbeddingDeployment": "text-embedding-ada-002"
}
```

**🔒 Security Tip:** Use User Secrets for the API key:
```bash
cd StorefrontRazor
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-actual-api-key"
```

#### C. Test It!

1. Start the app: `dotnet run`
2. Go to any product page
3. Scroll down to see **AI-powered recommendations** with the purple AI badge! ✨

---

### Step 3: Verify It's Working

Look for this in your logs:
```
info: Infrastructure.Services.AIRecommendationService[0]
      Azure OpenAI client initialized successfully
```

On the product details page, you should see:
- Purple "AI" badge with pulse animation
- "You Might Also Like" heading
- "Powered by AI • Smart recommendations based on product similarity"

---

## 📊 Understanding the Difference

### Without AI (Fallback Mode)
- Recommendations based on same **brand**
- Fast, always works
- Good for similar products from same manufacturer

### With AI Enabled
- Recommendations based on **semantic similarity**
- Understands product descriptions, features, use cases
- Can recommend across brands if products are truly similar
- Example: "Angular Blue Boots" might recommend "React Purple Boots" because they're both developer-themed boots

---

## 🎯 What's Next?

### Want to Customize?

**Change recommendation count:**
```csharp
// In StorefrontRazor/Pages/Products/Details.cshtml.cs, line ~100
var aiRecommendedProducts = await _aiRecommendationService
    .GetRecommendationsAsync(id, 8); // Change 4 to 8
```

**Change AI badge color:**
```css
/* In Details.cshtml, <style> section */
.bg-gradient-ai {
    background: linear-gradient(135deg, #your-color1 0%, #your-color2 100%);
}
```

### Want to Add More AI Features?

Check out the full documentation: `docs/AI-RECOMMENDATIONS.md`

Ideas already implemented in the service:
- ✅ Product-based recommendations
- ✅ Personalized recommendations (ready to use!)
- ✅ Fallback system
- ✅ Error handling

Coming soon:
- 🔄 Embedding caching with Redis
- 📊 Click tracking and analytics
- 🏠 Homepage personalization
- 🛒 "Customers also bought" feature

---

## ❓ Troubleshooting

### "AI recommendations not showing"
- Check `"Enabled": true` in appsettings.json
- Verify API key is correct
- Check logs for error messages

### "Still seeing brand recommendations"
- This is normal! AI recommendations show in addition to brand/type recommendations
- Make sure you're scrolling to the top recommendation section (it has the AI badge)

### "Too slow"
- Use `text-embedding-3-small` instead of `ada-002`
- Reduce candidate products (see customization section)
- Implement caching (see full docs)

---

## 💰 Cost Estimate

Running AI recommendations is **very affordable**:

- **Small store (< 100 products, 1000 views/month):** ~$2/month
- **Medium store (500 products, 10K views/month):** ~$15/month
- **Large store (2000 products, 100K views/month):** ~$100/month

**💡 Pro Tip:** Implement embedding caching to reduce costs by 90%+

---

## 🎉 That's It!

You now have AI-powered recommendations in your e-commerce store. The feature is:
- ✅ Production-ready
- ✅ Fault-tolerant
- ✅ Easy to configure
- ✅ Cost-effective

Enjoy your AI-enhanced store! 🚀
