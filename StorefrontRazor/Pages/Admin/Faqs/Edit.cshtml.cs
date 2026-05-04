using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages.Admin.Faqs;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public EditModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    [BindProperty]
    public FaqItem Faq { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Редактировать FAQ";
        var faqItem = await _unitOfWork.Repository<FaqItem>().GetByIdAsync(id);
        if (faqItem == null)
        {
            return NotFound();
        }
        Faq = faqItem;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _unitOfWork.Repository<FaqItem>().Update(Faq);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}