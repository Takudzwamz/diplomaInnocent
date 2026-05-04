using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages.Admin.Faqs;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    [BindProperty]
    public FaqItem Faq { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Создать FAQ";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _unitOfWork.Repository<FaqItem>().Add(Faq);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}