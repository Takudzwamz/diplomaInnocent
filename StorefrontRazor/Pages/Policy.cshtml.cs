using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages;

public class PolicyModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public PolicyModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public ContentBlock? PolicyContent { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Политика возврата";
        var spec = new BaseSpecification<ContentBlock>(cb => cb.Key == "return-policy");
        PolicyContent = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(spec);
    }
}