using System.Text.Json;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StorefrontRazor.Pages.Admin.Marketing;

public class IndexModel : PageModel
{
    private readonly IEmailQueueService _emailQueueService;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IEmailQueueService emailQueueService, IUnitOfWork unitOfWork)
    {
        _emailQueueService = emailQueueService;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public EmailCampaignModel Campaign { get; set; } = new();
    
    public SelectList EmailTemplates { get; set; } = default!;
    public string TemplatesJson { get; set; } = "{}";

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Email-маркетинг";
        var templates = await _unitOfWork.Repository<EmailTemplate>().ListAllAsync();
        
        EmailTemplates = new SelectList(templates, "Id", "Name");
        
        var templatesData = templates.ToDictionary(t => t.Id, t => new { t.Subject, t.Body });
        TemplatesJson = JsonSerializer.Serialize(templatesData);
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var message = new EmailMessage { Subject = Campaign.Subject, Body = Campaign.Body };
        _emailQueueService.QueueEmail(message);

        StatusMessage = "Success! Your email campaign has been queued and will be sent to all customers in the background.";
        return RedirectToPage();
    }
}