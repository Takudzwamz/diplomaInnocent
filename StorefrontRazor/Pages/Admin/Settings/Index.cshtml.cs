using System.ComponentModel.DataAnnotations;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Settings;

public class SettingsViewModel
{
    public string? StoreName { get; set; }
    public string? CurrentLogoUrl { get; set; }
    public string? CurrentFaviconUrl { get; set; }

    [Required]
    [Url]
    [Display(Name = "Публичный URL сайта")]
    public string PublicUrl { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Admin Notification Email")]
    public string AdminNotificationEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "SendGrid API Key")]
    public string SendGridKey { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Cloudinary Cloud Name")]
    public string CloudinaryCloudName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Cloudinary API Key")]
    public string CloudinaryApiKey { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Cloudinary API Secret")]
    public string CloudinaryApiSecret { get; set; } = string.Empty;
}

public class IndexModel : PageModel
{
    private readonly ISiteSettingsService _settingsService;
    private readonly IImageService _imageService;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(ISiteSettingsService settingsService, IImageService imageService, IUnitOfWork unitOfWork)
    {
        _settingsService = settingsService;
        _imageService = imageService;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public SettingsViewModel Vm { get; set; } = new();
    
    [BindProperty]
    public IFormFile? LogoUpload { get; set; }

    [BindProperty]
    public IFormFile? FaviconUpload { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Site Settings";
        var settings = await _settingsService.GetSettingsAsync();
        Vm.StoreName = settings.GetValueOrDefault("StoreName");
        Vm.CurrentLogoUrl = settings.GetValueOrDefault("StoreLogoUrl");
        Vm.CurrentFaviconUrl = settings.GetValueOrDefault("StoreFaviconUrl");
        Vm.PublicUrl = settings.GetValueOrDefault("PublicUrl", "https://example.com") ?? string.Empty;
        Vm.AdminNotificationEmail = settings.GetValueOrDefault("AdminNotificationEmail", "admin@example.com") ?? string.Empty;
        
        Vm.SendGridKey = settings.GetValueOrDefault("SendGrid_ApiKey") ?? string.Empty;
        Vm.CloudinaryCloudName = settings.GetValueOrDefault("Cloudinary_CloudName") ?? string.Empty;
        Vm.CloudinaryApiKey = settings.GetValueOrDefault("Cloudinary_ApiKey") ?? string.Empty;
        Vm.CloudinaryApiSecret = settings.GetValueOrDefault("Cloudinary_ApiSecret") ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var repo = _unitOfWork.Repository<Core.Entities.SiteSetting>();

        await UpdateSettingAsync(repo, "StoreName", Vm.StoreName);
        await UpdateSettingAsync(repo, "PublicUrl", Vm.PublicUrl);
        await UpdateSettingAsync(repo, "AdminNotificationEmail", Vm.AdminNotificationEmail);

        await UpdateSettingAsync(repo, "SendGrid_ApiKey", Vm.SendGridKey);
        await UpdateSettingAsync(repo, "Cloudinary_CloudName", Vm.CloudinaryCloudName);
        await UpdateSettingAsync(repo, "Cloudinary_ApiKey", Vm.CloudinaryApiKey);
        await UpdateSettingAsync(repo, "Cloudinary_ApiSecret", Vm.CloudinaryApiSecret);

        // Handle Logo Upload
        if (LogoUpload != null)
        {
            var logoUrl = await _imageService.AddImageAsync(LogoUpload);
            var logoSetting = (await repo.ListAsync(new Core.Specifications.BaseSpecification<Core.Entities.SiteSetting>(s => s.Key == "StoreLogoUrl"))).FirstOrDefault();
            if (logoSetting != null) { logoSetting.Value = logoUrl; repo.Update(logoSetting); }
        }

        // Handle Favicon Upload
        if (FaviconUpload != null)
        {
            var faviconUrl = await _imageService.AddImageAsync(FaviconUpload);
            var faviconSetting = (await repo.ListAsync(new Core.Specifications.BaseSpecification<Core.Entities.SiteSetting>(s => s.Key == "StoreFaviconUrl"))).FirstOrDefault();
            if (faviconSetting != null) { faviconSetting.Value = faviconUrl; repo.Update(faviconSetting); }
        }

        await _unitOfWork.Complete();
        _settingsService.ClearCache(); // IMPORTANT: Clear the cache so changes appear immediately

        TempData["SuccessMessage"] = "Настройки сайта успешно сохранены!";

        return RedirectToPage();
    }
    
    private async Task UpdateSettingAsync(IGenericRepository<Core.Entities.SiteSetting> repo, string key, string? value)
    {
        var spec = new BaseSpecification<Core.Entities.SiteSetting>(s => s.Key == key);
        var setting = (await repo.ListAsync(spec)).FirstOrDefault();
        if (setting != null)
        {
            setting.Value = value ?? "";
            repo.Update(setting);
        }else
        {
            // Optional: Create the setting if it's missing
            repo.Add(new SiteSetting { Key = key, Value = value ?? "" });
        }
    }
}