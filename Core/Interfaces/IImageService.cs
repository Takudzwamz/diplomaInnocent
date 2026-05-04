using Microsoft.AspNetCore.Http;

namespace Core.Interfaces;

/// <summary>
/// Interface for a service that handles image uploads.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads an image file.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<string> AddImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string imageUrl);

}
