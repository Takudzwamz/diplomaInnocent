using Core.Entities;
using Core.Interfaces;
using Core.Specifications; // Add this using statement
using Microsoft.AspNetCore.Mvc; // Add this using statement
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Content;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public IndexModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    public IReadOnlyList<ContentBlock> ContentBlocks { get; set; } = new List<ContentBlock>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Content Management";
        ContentBlocks = await _unitOfWork.Repository<ContentBlock>().ListAllAsync();
    }

    public async Task<IActionResult> OnPostToggleVisibilityAsync(string key)
    {
        // Check if the checkbox was checked. If it's not, the form won't send the "isEnabled" value.
        bool isEnabled = Request.Form.ContainsKey("isEnabled");

        var spec = new BaseSpecification<ContentBlock>(cb => cb.Key == key);
        var block = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(spec);

        if (block == null) return NotFound();

        block.Content = isEnabled.ToString().ToLower(); // Save as "true" or "false"
        _unitOfWork.Repository<ContentBlock>().Update(block);
        await _unitOfWork.Complete();

        return RedirectToPage();
    }
}