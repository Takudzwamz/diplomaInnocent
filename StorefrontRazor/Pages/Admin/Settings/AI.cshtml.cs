using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Settings;

public class AIModel : PageModel
{
    private readonly ISiteSettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Infrastructure.Services.AzureOpenAIClientService _aiClientService;

    public AIModel(
        ISiteSettingsService settingsService, 
        IUnitOfWork unitOfWork,
        Infrastructure.Services.AzureOpenAIClientService aiClientService)
    {
        _settingsService = settingsService;
        _unitOfWork = unitOfWork;
        _aiClientService = aiClientService;
    }

    [BindProperty]
    public AISettingsInput Input { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        ViewData["Title"] = "AI Settings";
        var settings = await _settingsService.GetSettingsAsync();

        Input = new AISettingsInput
        {
            IsEnabled = (settings.GetValueOrDefault("AI_Enabled", "false") ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase),
            Endpoint = settings.GetValueOrDefault("AI_Endpoint", "") ?? "",
            ApiKey = settings.GetValueOrDefault("AI_ApiKey", "") ?? "",
            EmbeddingDeployment = settings.GetValueOrDefault("AI_EmbeddingDeployment", "text-embedding-ada-002") ?? "text-embedding-ada-002",
            ChatDeployment = settings.GetValueOrDefault("AI_ChatDeployment", "gpt-4o-mini") ?? "gpt-4o-mini"
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate that if AI is enabled, all required fields are provided
        if (Input.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(Input.Endpoint))
            {
                ModelState.AddModelError("Input.Endpoint", "Конечная точка Azure OpenAI обязательна при включённом ИИ.");
            }
            
            if (string.IsNullOrWhiteSpace(Input.ApiKey))
            {
                ModelState.AddModelError("Input.ApiKey", "API-ключ обязателен при включённом ИИ.");
            }
            
            if (string.IsNullOrWhiteSpace(Input.EmbeddingDeployment))
            {
                ModelState.AddModelError("Input.EmbeddingDeployment", "Имя Embedding-деплоя обязательно при включённом ИИ.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }
        }

        var settingsToUpdate = new Dictionary<string, string>
        {
            { "AI_Enabled", Input.IsEnabled.ToString().ToLower() },
            { "AI_Endpoint", Input.Endpoint ?? "" },
            { "AI_ApiKey", Input.ApiKey ?? "" },
            { "AI_EmbeddingDeployment", Input.EmbeddingDeployment ?? "text-embedding-ada-002" },
            { "AI_ChatDeployment", Input.ChatDeployment ?? "gpt-4o-mini" }
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
        
        // Refresh the AI client service to pick up new settings
        await _aiClientService.RefreshSettingsAsync();
        
        TempData["SuccessMessage"] = $"Настройки ИИ сохранены. ИИ-рекомендации теперь {(Input.IsEnabled ? "включены" : "выключены")}.";
        return RedirectToPage();
    }
}

public class AISettingsInput
{
    [Display(Name = "Включить ИИ-рекомендации")]
    public bool IsEnabled { get; set; }
    
    [Display(Name = "Azure OpenAI Endpoint")]
    public string? Endpoint { get; set; }
    
    [Display(Name = "API Key")]
    public string? ApiKey { get; set; }
    
    [Display(Name = "Embedding Deployment Name")]
    public string? EmbeddingDeployment { get; set; }

    [Display(Name = "Chat Deployment Name")]
    public string? ChatDeployment { get; set; }
}
