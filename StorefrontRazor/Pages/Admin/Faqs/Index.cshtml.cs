using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages.Admin.Faqs;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public IndexModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
    public IReadOnlyList<FaqItem> Faqs { get; set; } = new List<FaqItem>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление FAQ";
        var allFaqs = await _unitOfWork.Repository<FaqItem>().ListAllAsync();
        Faqs = allFaqs.OrderBy(f => f.DisplayOrder).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _unitOfWork.Repository<FaqItem>().GetByIdAsync(id);
        if (item != null)
        {
            _unitOfWork.Repository<FaqItem>().Remove(item);
            await _unitOfWork.Complete();
        }
        return RedirectToPage();
    }
}