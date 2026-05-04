using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Settings;

public class PaymentModel : PageModel
{
    private readonly ISiteSettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;

    public string PublicUrl { get; set; } = string.Empty;

    public PaymentModel(ISiteSettingsService settingsService, IUnitOfWork unitOfWork)
    {
        _settingsService = settingsService;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public PaymentSettingsInput Input { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Настройки оплаты";
        var settings = await _settingsService.GetSettingsAsync();

        Input = new PaymentSettingsInput
        {
            ActiveGateway = settings.GetValueOrDefault("Payment_ActiveGateway", "Paystack"),
            SiteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test"),

            PaystackTestSecret = settings.GetValueOrDefault("Paystack_Test_SecretKey"),
            PaystackTestPublic = settings.GetValueOrDefault("Paystack_Test_PublicKey"),
            PaystackLiveSecret = settings.GetValueOrDefault("Paystack_Live_SecretKey"),
            PaystackLivePublic = settings.GetValueOrDefault("Paystack_Live_PublicKey"),

            PayFastTestMerchantId = settings.GetValueOrDefault("PayFast_Test_MerchantId"),
            PayFastTestMerchantKey = settings.GetValueOrDefault("PayFast_Test_MerchantKey"),
            PayFastTestPassphrase = settings.GetValueOrDefault("PayFast_Test_Passphrase"),
            PayFastLiveMerchantId = settings.GetValueOrDefault("PayFast_Live_MerchantId"),
            PayFastLiveMerchantKey = settings.GetValueOrDefault("PayFast_Live_MerchantKey"),
            PayFastLivePassphrase = settings.GetValueOrDefault("PayFast_Live_Passphrase")
        };
        
        // Get the PublicUrl from settings to display on the page
        PublicUrl = settings.GetValueOrDefault("PublicUrl", "https-your-site.com");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var settingsToUpdate = new Dictionary<string, string>
        {
            { "Payment_ActiveGateway", Input.ActiveGateway },
            { "Payment_SiteMode", Input.SiteMode },
            { "Paystack_Test_SecretKey", Input.PaystackTestSecret ?? "" },
            { "Paystack_Test_PublicKey", Input.PaystackTestPublic ?? "" },
            { "Paystack_Live_SecretKey", Input.PaystackLiveSecret ?? "" },
            { "Paystack_Live_PublicKey", Input.PaystackLivePublic ?? "" },
            { "PayFast_Test_MerchantId", Input.PayFastTestMerchantId ?? "" },
            { "PayFast_Test_MerchantKey", Input.PayFastTestMerchantKey ?? "" },
            { "PayFast_Test_Passphrase", Input.PayFastTestPassphrase ?? "" },
            { "PayFast_Live_MerchantId", Input.PayFastLiveMerchantId ?? "" },
            { "PayFast_Live_MerchantKey", Input.PayFastLiveMerchantKey ?? "" },
            { "PayFast_Live_Passphrase", Input.PayFastLivePassphrase ?? "" }
        };

        var repo = _unitOfWork.Repository<Core.Entities.SiteSetting>();
        
        foreach (var (key, value) in settingsToUpdate)
        {
            var spec = new BaseSpecification<Core.Entities.SiteSetting>(s => s.Key == key);
            var setting = (await repo.ListAsync(spec)).FirstOrDefault();
            if (setting != null)
            {
                setting.Value = value;
                repo.Update(setting);
            }
        }
        
        await _unitOfWork.Complete();
        _settingsService.ClearCache();
        
        TempData["SuccessMessage"] = "Настройки оплаты успешно сохранены.";
        return RedirectToPage();
    }
}

public class PaymentSettingsInput
{
    [Required]
    [Display(Name = "Активный платёжный шлюз")]
    public string ActiveGateway { get; set; } = "Paystack";
    
    [Required]
    [Display(Name = "Режим сайта")]
    public string SiteMode { get; set; } = "Test";
    
    [Display(Name = "Тестовый секретный ключ Paystack")]
    public string? PaystackTestSecret { get; set; }
    [Display(Name = "Тестовый публичный ключ Paystack")]
    public string? PaystackTestPublic { get; set; }
    [Display(Name = "Боевой секретный ключ Paystack")]
    public string? PaystackLiveSecret { get; set; }
    [Display(Name = "Боевой публичный ключ Paystack")]
    public string? PaystackLivePublic { get; set; }
    
    [Display(Name = "Тестовый Merchant ID PayFast")]
    public string? PayFastTestMerchantId { get; set; }
    [Display(Name = "Тестовый Merchant Key PayFast")]
    public string? PayFastTestMerchantKey { get; set; }
    [Display(Name = "Тестовая парольная фраза PayFast")]
    public string? PayFastTestPassphrase { get; set; }
    [Display(Name = "Боевой Merchant ID PayFast")]
    public string? PayFastLiveMerchantId { get; set; }
    [Display(Name = "Боевой Merchant Key PayFast")]
    public string? PayFastLiveMerchantKey { get; set; }
    [Display(Name = "Боевая парольная фраза PayFast")]
    public string? PayFastLivePassphrase { get; set; }
}