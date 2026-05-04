using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Marketing.Templates;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    [BindProperty]
    public EmailTemplate Template { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        _unitOfWork.Repository<EmailTemplate>().Add(Template);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index"); // We'll assume an Index page exists
    }
}