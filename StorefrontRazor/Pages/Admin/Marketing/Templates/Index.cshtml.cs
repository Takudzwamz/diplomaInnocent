using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Marketing.Templates;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IReadOnlyList<EmailTemplate> Templates { get; set; } = new List<EmailTemplate>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление шаблонами писем";
        Templates = await _unitOfWork.Repository<EmailTemplate>().ListAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var template = await _unitOfWork.Repository<EmailTemplate>().GetByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<EmailTemplate>().Remove(template);
        await _unitOfWork.Complete();

        return RedirectToPage();
    }
}