using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StorefrontRazor.Pages;

public class SitemapModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISiteSettingsService _siteSettings;

    public string PublicUrl { get; set; } = string.Empty;
    public List<SitemapNode> SitemapNodes { get; set; } = new();

    public SitemapModel(IUnitOfWork unitOfWork, ISiteSettingsService siteSettings)
    {
        _unitOfWork = unitOfWork;
        _siteSettings = siteSettings;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Get the site's public URL from your settings
        var settings = await _siteSettings.GetSettingsAsync();
        PublicUrl = settings.GetValueOrDefault("PublicUrl")?.TrimEnd('/') ?? "https://sputnikdevsdemo.store";

        // 1. Add static pages
        SitemapNodes.Add(new SitemapNode { Url = PublicUrl, Priority = "1.0" });
        SitemapNodes.Add(new SitemapNode { Url = $"{PublicUrl}/Products", Priority = "0.9" });
        // (Add other static pages like /Contact, /Faq, etc. here)
        // SitemapNodes.Add(new SitemapNode { Url = $"{PublicUrl}/Faq", Priority = "0.7" });

        // 2. Add all Products
        var products = await _unitOfWork.Repository<Product>().ListAllAsync();
        foreach (var product in products)
        {
            SitemapNodes.Add(new SitemapNode { Url = $"{PublicUrl}/Products/{product.Id}", Priority = "0.8" });
        }
        
        // 3. Add all Categories
        var categories = await _unitOfWork.Repository<Category>().ListAllAsync();
        foreach (var category in categories)
        {
            SitemapNodes.Add(new SitemapNode { Url = $"{PublicUrl}/Products?categoryId={category.Id}", Priority = "0.7" });
        }
        
        // 4. Add all Brands
        var brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();
        foreach (var brand in brands)
        {
            SitemapNodes.Add(new SitemapNode { Url = $"{PublicUrl}/Products?brandId={brand.Id}", Priority = "0.7" });
        }

        // Set the content type to XML
        Response.ContentType = "application/xml";
        return Page();
    }
}

// Helper class
public class SitemapNode
{
    public string Url { get; set; } = string.Empty;
    public string LastModified { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Priority { get; set; } = "0.8"; // Default priority
}