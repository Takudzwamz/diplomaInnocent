using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Marketing.Templates;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public EditModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    [BindProperty]
    public EmailTemplate Template { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Редактировать шаблон письма";
        var template = await _unitOfWork.Repository<EmailTemplate>().GetByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }
        Template = template;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _unitOfWork.Repository<EmailTemplate>().Update(Template);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}