using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Content;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public EditModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    [BindProperty]
    public ContentBlock ContentBlock { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Edit Content Block";
        var block = await _unitOfWork.Repository<ContentBlock>().GetByIdAsync(id);
        if (block == null) return NotFound();
        
        ContentBlock = block;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        _unitOfWork.Repository<ContentBlock>().Update(ContentBlock);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}