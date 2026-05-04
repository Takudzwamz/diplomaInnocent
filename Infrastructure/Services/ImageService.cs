/* using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> AddImageAsync(IFormFile file)
    {
        if (file.Length <= 0)
        {
            return string.Empty;
        }

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            // Optional: Apply transformations, e.g., crop to a square
            Transformation = new Transformation().Height(500).Width(500).Crop("fill")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception(uploadResult.Error.Message);
        }

        return uploadResult.SecureUrl.ToString();
    }
    
     public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Extract the public ID from the full Cloudinary URL
            var publicId = Path.GetFileNameWithoutExtension(new Uri(imageUrl).AbsolutePath);
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }
}
 */

 using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // 1. Add this

namespace Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly Cloudinary? _cloudinary; // 2. Make nullable
    private readonly ILogger<ImageService> _logger; // 3. Add logger

    public ImageService(ISiteSettingsService settingsService, ILogger<ImageService> logger) // 4. Change constructor
    {
        _logger = logger;
        var settings = settingsService.GetSettingsAsync().Result; // Get settings
        
        var cloudName = settings.GetValueOrDefault("Cloudinary_CloudName");
        var apiKey = settings.GetValueOrDefault("Cloudinary_ApiKey");
        var apiSecret = settings.GetValueOrDefault("Cloudinary_ApiSecret");

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            _logger.LogWarning("Cloudinary settings are not fully configured. Image service will be disabled.");
            _cloudinary = null;
        }
        else
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
    }

    public async Task<string> AddImageAsync(IFormFile file)
    {
        if (_cloudinary == null) // 5. Check if configured
        {
            _logger.LogError("Cannot upload image: Cloudinary service is not configured.");
            return string.Empty;
        }
        
        if (file.Length <= 0) return string.Empty;

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            // Preserve quality: limit to max 1500px while maintaining aspect ratio
            // Quality auto:best for optimal quality, fetch_format auto for best format (WebP/AVIF when supported)
            Transformation = new Transformation()
                .Width(1500).Height(1500).Crop("limit")
                .Quality("auto:best")
                .FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new Exception(uploadResult.Error.Message);
        }
        return uploadResult.SecureUrl.ToString();
    }
    
    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        if (_cloudinary == null) // 6. Check if configured
        {
            _logger.LogError("Cannot delete image: Cloudinary service is not configured.");
            return false;
        }
        
        try
        {
            var publicId = Path.GetFileNameWithoutExtension(new Uri(imageUrl).AbsolutePath);
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from Cloudinary.");
            return false;
        }
    }
}