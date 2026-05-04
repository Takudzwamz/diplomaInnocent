using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages;

public class FaqModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public FaqModel(IUnitOfWork unitOfWork) 
    { 
        _unitOfWork = unitOfWork; 
    }

    public IReadOnlyList<FaqItem> Faqs { get; set; } = new List<FaqItem>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Часто задаваемые вопросы";
        ViewData["Description"] = "Ответы на частые вопросы о наших товарах, доставке, возвратах и многом другом. Получите помощь быстро.";
        
        // Create a specification to get only published FAQs, ordered by the DisplayOrder field.
        var spec = new BaseSpecification<FaqItem>(f => f.IsPublished, f => f.DisplayOrder);
        Faqs = await _unitOfWork.Repository<FaqItem>().ListAsync(spec);
    }
}