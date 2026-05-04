# AI Settings Admin Panel

## Overview

The AI settings have been moved from `appsettings.json` to the database, allowing administrators to configure Azure OpenAI integration through the admin panel without requiring application restarts or redeployment.

## Features

### 1. Database-Backed Configuration
All AI settings are now stored in the `SiteSettings` table:
- `AI_Enabled` - Enable/disable AI recommendations (true/false)
- `AI_Endpoint` - Azure OpenAI resource endpoint URL
- `AI_ApiKey` - Azure OpenAI API key (stored securely)
- `AI_EmbeddingDeployment` - Deployment name for embeddings model

### 2. Admin Panel Management
Navigate to: **Admin → Settings → AI Settings**

The admin interface provides:
- **Enable/Disable Toggle** - Turn AI recommendations on/off instantly
- **Endpoint URL** - Configure your Azure OpenAI resource endpoint
- **API Key Field** - Secure password field with show/hide toggle
- **Deployment Name** - Specify the embedding model deployment

### 3. Automatic Configuration Refresh
When you save settings in the admin panel:
1. Settings are updated in the database
2. Cache is cleared via `ISiteSettingsService.ClearCache()`
3. `AzureOpenAIClientService` is refreshed with new settings
4. Changes take effect immediately (no restart required)

## Migration from appsettings.json

### Old Configuration (appsettings.json)
```json
"AzureOpenAI": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "ApiKey": "your-api-key-here",
  "EmbeddingDeployment": "text-embedding-ada-002",
  "Enabled": false
}
```

### New Configuration (Database)
Settings are automatically seeded when you run the application:
```csharp
// StoreContextSeed.cs
new() { Key = "AI_Enabled", Value = "false" },
new() { Key = "AI_Endpoint", Value = "" },
new() { Key = "AI_ApiKey", Value = "" },
new() { Key = "AI_EmbeddingDeployment", Value = "text-embedding-ada-002" }
```

**Note:** You can remove the `AzureOpenAI` section from `appsettings.json` after migrating to database settings.

## Setup Instructions

### First-Time Setup
1. Navigate to **Admin Panel → Settings**
2. Click the **"Manage AI"** button on the AI Settings card
3. Configure your Azure OpenAI settings:
   - Enter your Azure OpenAI endpoint URL
   - Enter your API key
   - Verify the deployment name (default: `text-embedding-ada-002`)
4. Toggle **"Enable AI-Powered Recommendations"** to ON
5. Click **"Save AI Settings"**

### Obtaining Azure OpenAI Credentials
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure OpenAI resource
3. Go to **Keys and Endpoint** section
4. Copy:
   - **Endpoint** (e.g., `https://your-resource.openai.azure.com/`)
   - **Key 1** or **Key 2**
5. Ensure you have deployed the `text-embedding-ada-002` model in Azure OpenAI Studio

## Technical Details

### Service Architecture

**Before (Static Configuration):**
```
appsettings.json → IConfiguration → AzureOpenAIClientService (Singleton)
```

**After (Database Configuration):**
```
Database → ISiteSettingsService (Cached) → AzureOpenAIClientService (Singleton)
                                         ↓
                                    RefreshSettingsAsync()
```

### Key Components

1. **AzureOpenAIClientService** (Singleton)
   - Reads settings from `ISiteSettingsService` instead of `IConfiguration`
   - Initializes once at startup
   - Provides `RefreshSettingsAsync()` method for hot-reloading configuration
   - Located: `/Infrastructure/Services/AzureOpenAIClientService.cs`

2. **AI Settings Admin Page**
   - Located: `/StorefrontRazor/Pages/Admin/Settings/AI.cshtml`
   - Model: `/StorefrontRazor/Pages/Admin/Settings/AI.cshtml.cs`
   - Features:
     - Form validation (requires all fields when AI is enabled)
     - Password field with visibility toggle
     - Field disabling when AI is disabled
     - Success/error messaging

3. **Settings Seed**
   - Located: `/Infrastructure/Data/StoreContextSeed.cs`
   - Seeds default AI settings on first run
   - Only adds settings that don't already exist

### Cache Management

Settings are cached for 1 hour by `ISiteSettingsService`:
- Cache is automatically cleared when settings are updated
- `AzureOpenAIClientService` is refreshed after cache clear
- New settings are loaded from database immediately

### Security

- API keys are stored as plain text in database (same as payment gateway keys)
- Consider implementing encryption for sensitive settings in production
- Password field type prevents API key from being visible in browser
- Access restricted to admin users only

## Testing

### Verify AI Settings Work

1. **Enable AI:**
   - Go to Admin → Settings → AI Settings
   - Toggle AI to enabled
   - Enter valid Azure OpenAI credentials
   - Save settings

2. **Test Recommendations:**
   - Navigate to any product details page
   - Scroll to the bottom
   - Verify "AI-Powered Recommendations" section appears with purple badge

3. **Disable AI:**
   - Go back to AI Settings
   - Toggle AI to disabled
   - Save settings
   - Refresh product page
   - Verify AI section no longer appears

### Troubleshooting

**AI recommendations not showing:**
- Check if AI is enabled in Admin → Settings → AI Settings
- Verify Azure OpenAI credentials are correct
- Check application logs for errors (`AzureOpenAIClientService` initialization)
- Ensure your Azure OpenAI deployment name matches the setting

**Changes not taking effect:**
- Cache should be cleared automatically
- Try refreshing the page
- If issue persists, restart the application

## Future Enhancements

Potential improvements:
- [ ] Encrypt API keys in database
- [ ] Add "Test Connection" button in admin panel
- [ ] Show AI service status/health in admin dashboard
- [ ] Add configuration for recommendation count
- [ ] Support multiple AI models/deployments
- [ ] Add usage analytics for AI recommendations

## Benefits

✅ **No Application Restart** - Changes take effect immediately  
✅ **Merchant Self-Service** - Store owners can manage AI without developer help  
✅ **Centralized Configuration** - All settings in one admin panel  
✅ **Consistent Pattern** - Follows same pattern as payment gateway settings  
✅ **Production-Friendly** - No need to redeploy when updating API keys  

---

**Related Documentation:**
- [AI Recommendations Feature](./AI-RECOMMENDATIONS.md)
- [AI Quick Start Guide](./AI-QUICK-START.md)
- [AI Implementation Summary](./AI-IMPLEMENTATION-SUMMARY.md)
